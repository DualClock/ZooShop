-- =============================================
-- ZooShop — Принудительная активация всех стандартных пользователей
-- =============================================

USE ZooShop;
GO

PRINT '=== ДО исправления ===';
SELECT u.UserID, u.Login, r.RoleName, u.IsActive
FROM dbo.Users u
JOIN dbo.UserRoles r ON u.RoleID = r.RoleID;
GO

-- Активируем стандартных пользователей
UPDATE dbo.Users 
SET IsActive = 1
WHERE Login IN (N'maria@zooshop.ru', N'anna@zooshop.ru', N'ivan@zooshop.ru');

PRINT '';
PRINT 'Активированы: maria@zooshop.ru, anna@zooshop.ru, ivan@zooshop.ru';
GO

-- Убедимся что роли правильные
UPDATE u
SET u.RoleID = r.RoleID
FROM dbo.Users u
JOIN dbo.UserRoles r ON r.RoleName = CASE 
    WHEN u.Login = N'maria@zooshop.ru' THEN N'Админ'
    ELSE N'Сотрудник'
END
WHERE u.Login IN (N'maria@zooshop.ru', N'anna@zooshop.ru', N'ivan@zooshop.ru');

PRINT 'Роли проверены/исправлены';
GO

PRINT '';
PRINT '=== ПОСЛЕ исправления ===';
SELECT u.UserID, u.Login, u.[Password], r.RoleName, u.EmployeeID, u.IsActive
FROM dbo.Users u
JOIN dbo.UserRoles r ON u.RoleID = r.RoleID
WHERE u.Login IN (N'maria@zooshop.ru', N'anna@zooshop.ru', N'ivan@zooshop.ru');
GO
