SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpFeatureGroups]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpFeatures]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpFeatureValues]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[EtUserPlans]

SELECT * FROM [Esh3arTechDb_v101].[dbo].[AbpUsers]

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