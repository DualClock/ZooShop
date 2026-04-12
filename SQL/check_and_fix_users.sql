-- =============================================
-- ZooShop — Проверка и исправление пользователей и ролей
-- =============================================

USE ZooShop;
GO

-- 1. Проверяем роли
PRINT '=== Текущие роли ===';
SELECT * FROM dbo.UserRoles;
GO

-- 2. Проверяем пользователей
PRINT '=== Текущие пользователи ===';
SELECT u.UserID, u.Login, u.[Password], u.RoleID, r.RoleName, u.EmployeeID, u.IsActive
FROM dbo.Users u
LEFT JOIN dbo.UserRoles r ON u.RoleID = r.RoleID;
GO

-- 3. Проверяем сотрудников
PRINT '=== Текущие сотрудники ===';
SELECT EmployeeID, LastName, FirstName, MiddleName, Email, Position, Role, IsActive
FROM dbo.Employees;
GO

-- 4. Исправление: Убедимся что роли существуют
IF NOT EXISTS (SELECT * FROM dbo.UserRoles WHERE RoleName = N'Админ')
BEGIN
    INSERT INTO dbo.UserRoles (RoleName, Description) VALUES (N'Админ', N'Администратор системы');
    PRINT 'Роль "Админ" добавлена';
END
ELSE
BEGIN
    PRINT 'Роль "Админ" уже существует';
END

IF NOT EXISTS (SELECT * FROM dbo.UserRoles WHERE RoleName = N'Сотрудник')
BEGIN
    INSERT INTO dbo.UserRoles (RoleName, Description) VALUES (N'Сотрудник', N'Рядовой сотрудник');
    PRINT 'Роль "Сотрудник" добавлена';
END
ELSE
BEGIN
    PRINT 'Роль "Сотрудник" уже существует';
END
GO

-- 5. Исправление: Обновим/добавим пользователей
-- maria@zooshop.ru — Админ
IF EXISTS (SELECT * FROM dbo.Users WHERE Login = N'maria@zooshop.ru')
BEGIN
    UPDATE dbo.Users 
    SET RoleID = (SELECT RoleID FROM dbo.UserRoles WHERE RoleName = N'Админ'),
        IsActive = 1
    WHERE Login = N'maria@zooshop.ru';
    PRINT 'Пользователь maria@zooshop.ru обновлён (Админ)';
END
ELSE
BEGIN
    -- Ищем EmployeeID для Марии (если есть)
    DECLARE @mariaEmpId INT = (SELECT TOP 1 EmployeeID FROM dbo.Employees WHERE FirstName = N'Мария');
    
    INSERT INTO dbo.Users (Login, [Password], RoleID, EmployeeID, IsActive) 
    VALUES (N'maria@zooshop.ru', N'admin', 
            (SELECT RoleID FROM dbo.UserRoles WHERE RoleName = N'Админ'), 
            @mariaEmpId, 1);
    PRINT 'Пользователь maria@zooshop.ru добавлен (Админ)';
END
GO

-- anna@zooshop.ru — Сотрудник
IF EXISTS (SELECT * FROM dbo.Users WHERE Login = N'anna@zooshop.ru')
BEGIN
    UPDATE dbo.Users 
    SET RoleID = (SELECT RoleID FROM dbo.UserRoles WHERE RoleName = N'Сотрудник'),
        IsActive = 1
    WHERE Login = N'anna@zooshop.ru';
    PRINT 'Пользователь anna@zooshop.ru обновлён (Сотрудник)';
END
ELSE
BEGIN
    DECLARE @annaEmpId INT = (SELECT TOP 1 EmployeeID FROM dbo.Employees WHERE FirstName = N'Анна');
    
    INSERT INTO dbo.Users (Login, [Password], RoleID, EmployeeID, IsActive) 
    VALUES (N'anna@zooshop.ru', N'123', 
            (SELECT RoleID FROM dbo.UserRoles WHERE RoleName = N'Сотрудник'), 
            @annaEmpId, 1);
    PRINT 'Пользователь anna@zooshop.ru добавлен (Сотрудник)';
END
GO

-- ivan@zooshop.ru — Сотрудник
IF EXISTS (SELECT * FROM dbo.Users WHERE Login = N'ivan@zooshop.ru')
BEGIN
    UPDATE dbo.Users 
    SET RoleID = (SELECT RoleID FROM dbo.UserRoles WHERE RoleName = N'Сотрудник'),
        IsActive = 1
    WHERE Login = N'ivan@zooshop.ru';
    PRINT 'Пользователь ivan@zooshop.ru обновлён (Сотрудник)';
END
ELSE
BEGIN
    DECLARE @ivanEmpId INT = (SELECT TOP 1 EmployeeID FROM dbo.Employees WHERE FirstName = N'Иван');
    
    INSERT INTO dbo.Users (Login, [Password], RoleID, EmployeeID, IsActive) 
    VALUES (N'ivan@zooshop.ru', N'123', 
            (SELECT RoleID FROM dbo.UserRoles WHERE RoleName = N'Сотрудник'), 
            @ivanEmpId, 1);
    PRINT 'Пользователь ivan@zooshop.ru добавлен (Сотрудник)';
END
GO

-- 6. Финальная проверка
PRINT '';
PRINT '=== Итоговые роли ===';
SELECT RoleID, RoleName, Description FROM dbo.UserRoles;

PRINT '';
PRINT '=== Итоговые пользователи ===';
SELECT u.UserID, u.Login, u.[Password], r.RoleName, u.EmployeeID, u.IsActive
FROM dbo.Users u
JOIN dbo.UserRoles r ON u.RoleID = r.RoleID;
GO

PRINT 'Готово!';
GO
