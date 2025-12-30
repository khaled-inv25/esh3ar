using Esh3arTech.Messages.SendBehavior;
using Esh3arTech.MobileUsers;
using Esh3arTech.MobileUsers.Specs;
using Esh3arTech.Plans;
using Esh3arTech.Settings;
using Esh3arTech.Utility;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
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
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly UserPlanManager _userPlanManager;
        private readonly ICurrentUser _currentUser;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ISettingProvider _settingProvider;

        public OneWayMessageManager(
            IRepository<MobileUser, Guid> mobileUserRepository,
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

        public async Task<Message> CreateMessageAsync(string recipient, string content)
        {
            await _userPlanManager.CanSendMessageAsync(_currentUser.Id!.Value);

            await CheckExistanceOrVerifiedMobileNumberAsync(PrepareMobileNumber(recipient));

            var msgToReturn = new Message(_guidGenerator.Create(), $"967{recipient}", MessageType.OneWay); ;
            
            msgToReturn.SetSubject("default");

            if (string.IsNullOrEmpty(content))
            {
                throw new UserFriendlyException("Message content with no attachment can't be empty!");
            }

            msgToReturn.SetMessageContentOrNull(content);

            return msgToReturn;
        }

        public async Task<Message> CreateMessageWithAttachmentAsync()
        {
            return null;
        }

        public async Task<Message> CreateMessageWithAttachmentFromUiAsync(string recipient, string? content, IRemoteStreamContent stream)
        {
            await _userPlanManager.CanSendMessageAsync(_currentUser.Id!.Value);

            await CheckExistanceOrVerifiedMobileNumberAsync(PrepareMobileNumber(recipient));

            var msgToReturn = new Message(_guidGenerator.Create(), $"967{recipient}", MessageType.OneWay);
            msgToReturn.SetSubject("default");
            msgToReturn.SetMessageContentOrNull(content ?? null);

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

        private string PrepareMobileNumber(string mobileNumber) => MobileNumberPreparator.PrepareMobileNumber(mobileNumber);

        private async Task CheckExistanceOrVerifiedMobileNumberAsync(string mobileNumber)
        {
            if (!await _mobileUserRepository.AnyAsync(new MobileVerifiedSpecification(mobileNumber).ToExpression()))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
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
    }
}
