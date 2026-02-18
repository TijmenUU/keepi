INSERT INTO Users
("ExternalId", "EmailAddress", "Name", "IdentityOrigin", "EntriesPermission", "ExportsPermission", "ProjectsPermission", "UsersPermission")
VALUES (1337,'jantje@example.com','Jantje',1,2,1,1,1);
--
UPDATE users
SET 
    EntriesPermission = 2,
    ExportsPermission = 2,
    ProjectsPermission = 2,
    UsersPermission = 2
WHERE
    ExternalId = 8997518;
-- 
UPDATE userentries SET UserId = 2;