-- UAE Cities and States - Run this on Production Database
-- Date: 2026-01-21

-- Add remaining UAE Emirates as states (if not already present)
INSERT INTO "States" ("Name", "Code", "CountryId", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Sharjah', 'SHJ', 4, true, false, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "States" WHERE "Name" = 'Sharjah' AND "CountryId" = 4);

INSERT INTO "States" ("Name", "Code", "CountryId", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Ajman', 'AJM', 4, true, false, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "States" WHERE "Name" = 'Ajman' AND "CountryId" = 4);

INSERT INTO "States" ("Name", "Code", "CountryId", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Ras Al Khaimah', 'RAK', 4, true, false, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "States" WHERE "Name" = 'Ras Al Khaimah' AND "CountryId" = 4);

INSERT INTO "States" ("Name", "Code", "CountryId", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Fujairah', 'FUJ', 4, true, false, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "States" WHERE "Name" = 'Fujairah' AND "CountryId" = 4);

INSERT INTO "States" ("Name", "Code", "CountryId", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Umm Al Quwain', 'UAQ', 4, true, false, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "States" WHERE "Name" = 'Umm Al Quwain' AND "CountryId" = 4);

-- Add UAE cities (after ensuring states exist)
-- Dubai Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Dubai', 'DXB', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Dubai' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Dubai' AND "StateId" = s."Id");

INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Jebel Ali', 'JBL', s."Id", 4, false, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Dubai' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Jebel Ali' AND "StateId" = s."Id");

INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Dubai Marina', 'DXM', s."Id", 4, false, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Dubai' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Dubai Marina' AND "StateId" = s."Id");

-- Abu Dhabi Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Abu Dhabi', 'AUH', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Abu Dhabi' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Abu Dhabi' AND "StateId" = s."Id");

INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Al Ain', 'AAN', s."Id", 4, false, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Abu Dhabi' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Al Ain' AND "StateId" = s."Id");

-- Sharjah Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Sharjah', 'SHJ', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Sharjah' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Sharjah' AND "StateId" = s."Id");

INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Al Dhaid', 'DHD', s."Id", 4, false, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Sharjah' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Al Dhaid' AND "StateId" = s."Id");

-- Ajman Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Ajman', 'AJM', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Ajman' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Ajman' AND "StateId" = s."Id");

-- Ras Al Khaimah Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Ras Al Khaimah', 'RAK', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Ras Al Khaimah' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Ras Al Khaimah' AND "StateId" = s."Id");

-- Fujairah Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Fujairah', 'FUJ', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Fujairah' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Fujairah' AND "StateId" = s."Id");

-- Umm Al Quwain Emirate cities
INSERT INTO "Cities" ("Name", "Code", "StateId", "CountryId", "IsHub", "IsActive", "IsDeleted", "CreatedAt", "ModifiedAt")
SELECT 'Umm Al Quwain', 'UAQ', s."Id", 4, true, true, false, NOW(), NOW()
FROM "States" s WHERE s."Name" = 'Umm Al Quwain' AND s."CountryId" = 4
AND NOT EXISTS (SELECT 1 FROM "Cities" WHERE "Name" = 'Umm Al Quwain' AND "StateId" = s."Id");
