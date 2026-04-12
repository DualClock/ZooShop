-- =============================================
-- ZooShop — Заполнение таблицы Users
-- Запускать: sqlcmd -S "(localdb)\MSSQLLocalDB" -d ZooShop -i fill_users.sql
-- =============================================

USE ZooShop;
GO

-- Если Users уже есть — удаляем только её (UserRoles оставляем)
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- =============================================
-- Таблица пользователей
-- =============================================
CREATE TABLE dbo.Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Login NVARCHAR(100) NOT NULL UNIQUE,
    [Password] NVARCHAR(200) NOT NULL,
    RoleID INT NOT NULL,
    EmployeeID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Users_UserRole FOREIGN KEY (RoleID)
        REFERENCES dbo.UserRoles(RoleID),
    CONSTRAINT FK_Users_Employee FOREIGN KEY (EmployeeID)
        REFERENCES dbo.Employees(EmployeeID) ON DELETE SET NULL
);
GO

CREATE INDEX IX_Users_Login ON dbo.Users(Login);
GO

-- =============================================
-- Заполняем Users для существующих Employees
-- =============================================
INSERT INTO dbo.Users (Login, [Password], RoleID, EmployeeID, IsActive) VALUES
(N'maria@zooshop.ru', N'admin', 1, 3, 1),
(N'anna@zooshop.ru',  N'123',   2, 1, 1),
(N'ivan@zooshop.ru',  N'123',   2, 2, 1);
GO

-- Добавляем ещё 7 сотрудников для заполнения
INSERT INTO dbo.Users (Login, [Password], RoleID, EmployeeID, IsActive) VALUES
(N'petrov@zooshop.ru',   N'123', 2, 4, 1),
(N'sidorova@zooshop.ru', N'123', 2, 5, 1),
(N'volkov@zooshop.ru',   N'123', 2, 6, 1);
GO

-- =============================================
-- Проверка
-- =============================================
PRINT '=== Users ===';
SELECT u.UserID, u.Login, u.[Password], r.RoleName, u.EmployeeID, u.IsActive
FROM dbo.Users u
JOIN dbo.UserRoles r ON u.RoleID = r.RoleID;
GO
