using AutoMapper.Internal.Mappers;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Esh3arTech.Messages.SendBehavior;
using Esh3arTech.MobileUsers;
using Esh3arTech.MobileUsers.Specs;
using Esh3arTech.Plans;
using Esh3arTech.Settings;
using Esh3arTech.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Esh3arTech.Messages
{
    public class OneWayMessageManager : DomainService, IOneWayMessageManager
    {
        #region Fields

        private readonly IMobileUserRepository _mobileUserRepository;
        private readonly UserPlanManager _userPlanManager;
        private readonly ICurrentUser _currentUser;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ISettingProvider _settingProvider;

        #endregion

        #region Ctor
        public OneWayMessageManager(
            IMobileUserRepository mobileUserRepository,
            UserPlanManager userPlanManager,
            ICurrentUser currentUser,
            IGuidGenerator guidGenerator,
            ISettingProvider settingProvider)
        {
            _mobileUserRepository = mobileUserRepository;
            _userPlanManager = userPlanManager;
            _currentUser = currentUser;
            _guidGenerator = guidGenerator;
            _settingProvider = settingProvider;
        }

        #endregion

        #region Methods

        public async Task<List<Message>> CreateBatchMessageAsync(List<BatchMessage> data, List<EtTempMobileUserData> numbers)
        {
            var messages = new List<Message>();

            await _userPlanManager.CanSendMessageAsync(_currentUser.Id!.Value, data.Count);

            await CheckMissingOrNotVerifiedMobileNumberAsync(numbers);

            foreach (var item in data)
            {
                var msg = CreateMessage(item.MobileNumber, item.MessageContent!, _currentUser.Id!.Value);
                msg.CreationTime = DateTime.Now;

                messages.Add(msg);
            }

            return messages;
        }

        public async Task<Message> CreateMessageAsync(string recipient, string content)
        {
            var currentUserId = _currentUser.Id!.Value;
            await _userPlanManager.CanSendMessageAsync(_currentUser.Id!.Value);

            await CheckExistanceOrVerifiedMobileNumberAsync(PrepareMobileNumber(recipient));

            return CreateMessage(recipient, content, currentUserId);
        }

        public async Task<Message> CreateMessageWithAttachmentFromUiAsync(string recipient, string? content, IRemoteStreamContent stream)
        {
            await _userPlanManager.CanSendMessageAsync(_currentUser.Id!.Value);

            await CheckExistanceOrVerifiedMobileNumberAsync(PrepareMobileNumber(recipient));

            var msgToReturn = new Message(_guidGenerator.Create(), $"967{recipient}", MessageType.OneWay);
            msgToReturn.SetSubject("default");
            msgToReturn.SetMessageContentOrNull(content ?? null);
            msgToReturn.SetMessageStatusType(MessageStatus.Queued);

            var size = CalculateStreamSize(stream);
            var extension = Path.GetExtension(stream.FileName);

            if (size > MessageConts.MaxFileSize)
            {
                throw new BusinessException("Size is begger than 1Mb try other file!");
            }

            var attachmentId = _guidGenerator.Create();
            var accessUrl = await _settingProvider.GetOrNullAsync(Esh3arTechSettings.Blob.AccessUrl) ?? throw new ArgumentNullException("Url is null!");

            msgToReturn.AddAttachment(attachmentId, GenerateFileName(attachmentId, extension), stream.ContentType, accessUrl);

            return msgToReturn;
        }

        public async Task<List<Message>> CreateMessagesFromFileAsync(IRemoteStreamContent stream)
        {
            var size = CalculateStreamSize(stream);

            if (size > MessageConts.MaxFileSize)
            {
                throw new BusinessException("Size is begger than 1Mb try other file!");
            }

            var messagesTuple = ReadExcel(stream.GetStream());

            return await CreateBatchMessageAsync(messagesTuple.Item2, messagesTuple.Item1);
        }

        private Message CreateMessage(string recipient, string content, Guid currentUserId)
        {
            var msgToReturn = new Message(_guidGenerator.Create(), $"967{recipient}", MessageType.OneWay);

            msgToReturn.SetSubject("default");

            if (string.IsNullOrEmpty(content))
            {
                throw new UserFriendlyException("Message content with no attachment can't be empty!");
            }

            msgToReturn.SetMessageContentOrNull(content);
            msgToReturn.SetMessageStatusType(MessageStatus.Queued);
            msgToReturn.CreatorId = currentUserId;

            return msgToReturn;
        }

        private string PrepareMobileNumber(string mobileNumber) => MobileNumberPreparator.PrepareMobileNumber(mobileNumber);

        private async Task CheckExistanceOrVerifiedMobileNumberAsync(string mobileNumber)
        {
            if (!await _mobileUserRepository.AnyAsync(new MobileVerifiedSpecification(mobileNumber).ToExpression()))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }
        }

        private async Task CheckMissingOrNotVerifiedMobileNumberAsync(List<EtTempMobileUserData> numbers) 
        {
            var missingNumbers = await _mobileUserRepository.CheckExistanceOrVerifiedMobileNumberAsync(numbers);

            if (missingNumbers.Count > 0)
            {
                throw new BusinessException("There are missing or not verified numbers!")
                    .WithData("missing or not verified numbers", JsonSerializer.Serialize(missingNumbers));
            }
        }

        private long CalcBase64SizeInMb(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return 0;
            }

            int length = base64.Length;

            int padding = 0;

            if (base64.EndsWith("=="))
            {
                padding = 2;
            }
            else if (base64.EndsWith("="))
            {
                padding = 1;
            }

            long sizeInBytes = (long)(length * 3.0 / 4.0) - padding;
            long sizeInMb = (long)(sizeInBytes / (1024 * 1024));

            return sizeInMb;
        }

        private long CalculateStreamSize(IRemoteStreamContent streamContent)
        {
            if (streamContent == null)
            {
                return 0;
            }

            if (streamContent.ContentLength.HasValue)
            {
                return streamContent.ContentLength.Value;
            }


            using var stream = streamContent.GetStream();
            return stream.CanSeek ? stream.Length : 0;
        }

        private string GenerateFileName(Guid msgId, string extension)
        {
            return $"{msgId}{extension}";
        }

        private (List<EtTempMobileUserData>, List<BatchMessage>) ReadExcel(Stream stream)
        {
            (List<EtTempMobileUserData>, List<BatchMessage>) tuple = ([], []);

            int typeCounter = 0;
            CellType cellType = CellType.Number;

            using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
            WorkbookPart workbookPart = doc.WorkbookPart;

            Sheet sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

            foreach (Row row in sheetData.Elements<Row>())
            {
                var mobileNumber = string.Empty;
                var message = string.Empty;
                var subject = string.Empty;

                foreach (Cell cell in row.Elements<Cell>())
                {

                    string value = GetCellValue(doc, cell);
                    cellType = (CellType)(typeCounter % 3);

                    switch(cellType)
                    {
                        case CellType.Number:
                            mobileNumber = value;
                            break;
                        case CellType.Message:
                            message = value;
                            break;
                        case CellType.Subject:
                            subject = value;
                            break;
                    }

                    typeCounter++;
                }

                tuple.Item1.Add(new EtTempMobileUserData() { MobileNumber = MobileNumberPreparator.PrepareMobileNumber(mobileNumber) });
                tuple.Item2.Add(new BatchMessage() { MobileNumber = mobileNumber, MessageContent = message, Subject = subject });
            }


            return tuple;
        }

        private string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue?.InnerText;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable
                          .ChildElements[int.Parse(value)].InnerText;
            }
            return value;
        }

        private enum CellType { Number = 0, Message = 1, Subject = 2 }
        #endregion
    }
}
