SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpFeatureGroups]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpFeatures]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpFeatureValues]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[EtUserPlans]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpUsers]

SELECT 
	CASE
		WHEN
			ISNULL(EtUserPlans.DailyPrice, 0) = 0 AND
			ISNULL(EtUserPlans.WeeklyPrice, 0) = 0 AND
			ISNULL(EtUserPlans.MonthlayPrice, 0) = 0 AND
			ISNULL(EtUserPlans.AnnualPrice, 0) = 0 
		THEN 1
		ELSE 0
	END AS IsFree
FROM EtUserPlans

--UPDATE  [Esh3arTechDb_v101].[dbo].[AbpUsers] SET PlanId = '5F82439A-8516-D297-8ADE-3A1DC7246E18' Where Id = '51076BC4-E24A-1B43-A9A3-3A1DC2F8E982'

--UPDATE  [Esh3arTechDb_v101].[dbo].[AbpUsers] SET PlanId = null

--UPDATE  [Esh3arTechDb_v101].[dbo].[EtUserPlans] SET IsDeleted = 0

--TRUNCATE TABLE [Esh3arTechDb_v101].[dbo].[AbpFeatureGroups]

--TRUNCATE TABLE [Esh3arTechDb_v101].[dbo].[AbpFeatures]

--TRUNCATE TABLE [Esh3arTechDb_v101].[dbo].[AbpFeatureValues]


-- Users section
SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpUsers]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpUserRoles]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpRoles]

-- Plan Section
SELECT * FROM [Esh3arTechDb_v101].[dbo].[EtUserPlans]

TRUNCATE TABLE [Esh3arTechDb_v101].[dbo].[EtUserPlans]

Insert into EtUserPlans (Id, Name, DisplayName, CreationTime, CreatorId, IsDeleted)
values(NEWID(), 'primeum', 'Primeum', GETDATE(), '3AA83D86-A3D9-14A1-83E5-3A1D8A462EFD', 0);

Insert into EtUserPlans (Id, Name, DisplayName, CreationTime, CreatorId, IsDeleted)
values(NEWID(), 'free', 'Free', GETDATE(), '3AA83D86-A3D9-14A1-83E5-3A1D8A462EFD', 0);

UPDATE EtUserPlans SET ExpiringPlanId = '1C78910F-F10C-602B-CCFA-3A1DC7235E7F' Where Id = '5F82439A-8516-D297-8ADE-3A1DC7246E18'

UPDATE EtUserPlans SET AnnualPrice = 60 Where Id = 'D55FB645-A065-44E8-964D-87780C2E9982'

SELECT up.Name, up.DisplayName, sup.DisplayName as ExpireName from EtUserPlans up
left join EtUserPlans sup on up.ExpiringPlanId = sup.Id


-- Subscriptions sections
SELECT UserName, EtUserPlans.Name, BillingInterval, Price, StartDate, EndDate, NextBill, EtUserPlans.CreationTime
FROM EtSubscriptions
Join AbpUsers on EtSubscriptions.UserId = AbpUsers.Id
Join EtUserPlans on EtSubscriptions.PlanId = EtUserPlans.Id
order by CreationTime DESC

SELECT * FROM EtSubscriptions Where ID = 'E59F998D-6358-DEED-40EE-3A1E194CBAC8'

SELECT * FROM EtSubscriptionRenewalHistory WHERE SubscriptionId = 'E59F998D-6358-DEED-40EE-3A1E194CBAC8'

SELECT UserName, EtUserPlans.Name, EtSubscriptionRenewalHistory.* 
FROM EtSubscriptions
Join AbpUsers on EtSubscriptions.UserId = AbpUsers.Id
Join EtUserPlans on EtSubscriptions.PlanId = EtUserPlans.Id
Join EtSubscriptionRenewalHistory on EtSubscriptions.Id = EtSubscriptionRenewalHistory.SubscriptionId
Where EtSubscriptionRenewalHistory.SubscriptionId = 'E59F998D-6358-DEED-40EE-3A1E194CBAC8'

SELECT * FROM AbpUsers

DELETE FROM EtSubscriptions

TRUNCATE TABLE EtSubscriptionRenewalHistory

UPDATE AbpUsers SET PlanId = null

UPDATE EtSubscriptions SET StartDate = DATEADD(DAY, -1, StartDate)

UPDATE EtSubscriptions SET EndDate = DATEADD(DAY, -1, EndDate)

UPDATE EtSubscriptions SET NextBill = DATEADD(DAY, -1, NextBill)

UPDATE EtSubscriptionRenewalHistory SET RenewalDate = DATEADD(DAY, -1, RenewalDate)

UPDATE EtSubscriptionRenewalHistory SET PeriodStartDate = DATEADD(DAY, -1, PeriodStartDate)

UPDATE EtSubscriptionRenewalHistory SET PeriodEndDate = DATEADD(DAY, -1, PeriodEndDate)

SELECT * FROM EtUserPlans

UPDATE EtSubscriptions SET IsActive = 1

-- Messages sections
-- Pending = 0,    // Created, not yet sent
-- Sent = 1,       // Enqueued for delivery
-- Delivered = 2,  // Received by recipient device
-- Read = 3,       // Opened by recipient
-- Queued = 4,      // Waiting for action
-- Failed = 5      // Delivery failed after retries

SELECT * FROM AbpUsers
SELECT * FROM EtMobileUsers

-- I want a query to check amoung data I will provide if a specific recored is not applyed

SELECT IIF (
	(SELECT COUNT(*) FROM EtMobileUsers WHERE MobileNumber IN ('967775265496', '967775265499', '967775265492') ) = (SELECT COUNT(*) FROM EtMobileUsers),
	CAST(1 AS BIT),
	CAST(0 AS BIT)) AS IsAnyNumberFound;

SELECT CAST ( 
	CASE 
		WHEN NOT EXISTS (
		SELECT v.MobileNumber FROM (VALUES ('967775265496'), ('967775265497'), ('967775265498')) v(MobileNumber)  WHERE NOT EXISTS(SELECT 1 FROM EtMobileUsers e WHERE e.MobileNumber = v.MobileNumber))
		THEN 1
		ELSE 0
	END AS BIT)

SELECT TOP 1 1 FROM EtMobileUsers WHERE EXISTS(SELECT 1 FROM EtMobileUsers)

SELECT 
    CASE 
        WHEN Status = 0 THEN 'Pending'
        WHEN Status = 1 THEN 'Sent'
        WHEN Status = 2 THEN 'Delivered'
        WHEN Status = 3 THEN 'Read'
        WHEN Status = 4 THEN 'Queued'
        WHEN Status = 5 THEN 'Failed'
        ELSE 'Unknown'
    END AS StatusDescription,
	EtMessages.*
FROM EtMessages ORDER BY CreationTime DESC

SELECT 
	MessageContent,
	RecipientPhoneNumber,
	 CASE 
        WHEN Status = 0 THEN 'Pending'
        WHEN Status = 1 THEN 'Sent'
        WHEN Status = 2 THEN 'Delivered'
        WHEN Status = 3 THEN 'Read'
        WHEN Status = 4 THEN 'Queued'
        WHEN Status = 5 THEN 'Failed'
        ELSE 'Unknown'
    END AS StatusDescription,
	RetryCount,
	LastRetryAt,
	NextRetryAt,
	FailureReason,
	CreatorId,
	CreationTime
	FROM EtMessages ORDER BY CreationTime DESC

SELECT * FROM EtMessageAttachments

UPDATE EtMessages SET CreatorId = 'DAD566BE-82EF-E9FD-4584-3A1E7B04D314'

DELETE EtMessages WHERE CreatorId Is Null

SELECT COUNT(*) FROM EtMessages


UPDATE EtMessages SET CreatorId = 'DAD566BE-82EF-E9FD-4584-3A1E7B04D314'

SELECT * FROM EtMessages ORDER BY CreationTime DESC

DELETE FROM EtMessages 

SELECT * FROM EtMessages WHERE Id = '0D429AAC-FAD9-3187-115B-3A1E7210D640'


SELECT * FROM EtMessageAttachments

DELETE FROM EtMessages

DELETE FROM EtMessageAttachments

update EtMessages set Status = 2

SELECT * FROM EtMessages

SELECT COUNT(*) FROM EtMessages



UPDATE EtMessages SET Status = 2
-- Mobile sections
SELECT * FROM AbpUsers
SELECT * FROM EtMobileUsers

SELECT * FROM EtMobileUsers
-- INSERT INTO EtMobileUsers (Id, MobileNumber, Status, IsStatic, CreationTime, IsDeleted)
-- VALUES 
-- (NEWID(), '967775265496', 1, 0, GETDATE(), 0),
-- (NEWID(), '967775265497', 1, 0, GETDATE(), 0),
-- (NEWID(), '967775265498', 1, 0, GETDATE(), 0),
-- (NEWID(), '967775265499', 1, 0, GETDATE(), 0)