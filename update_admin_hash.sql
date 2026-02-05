USE PNHDigitalDB;
GO

UPDATE AdminUsers
SET SenhaHash = '$2a$11$ciHe11q3vtypJy0o9t257e9avxmqX1O8c36hkG2oGo3bZxrDDXpxm'
WHERE Email = 'admin@pnhdigital.com';
GO

SELECT Email, SenhaHash
FROM AdminUsers
WHERE Email = 'admin@pnhdigital.com';
GO
