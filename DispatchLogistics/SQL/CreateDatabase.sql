/* ============================================================
   Диспетчерская логистика: Заказы и тарифы
   SQL-скрипт создания базы данных
   Совместимость: SQL Server 2019 / SSMS 2019
   ============================================================ */

-- Переключаемся на master для проверки/создания БД
USE master;
GO

-- Удаляем БД если существует (для повторного запуска)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DispatchLogisticsDB')
BEGIN
    ALTER DATABASE DispatchLogisticsDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DispatchLogisticsDB;
    PRINT 'База данных DispatchLogisticsDB удалена.';
END
GO

-- Создаём базу данных
CREATE DATABASE DispatchLogisticsDB;
GO

USE DispatchLogisticsDB;
GO

PRINT 'База данных DispatchLogisticsDB создана.';
GO

/* ============================================================
   1. Таблица Users — Пользователи системы
   ============================================================ */
CREATE TABLE Users
(
    UserId          INT IDENTITY(1,1)       NOT NULL,
    Login           NVARCHAR(50)            NOT NULL,
    PasswordHash    NVARCHAR(255)           NOT NULL,
    FullName        NVARCHAR(150)           NOT NULL,
    [Role]          NVARCHAR(50)            NOT NULL,  -- 'Администратор' / 'Диспетчер'
    IsActive        BIT                     NOT NULL DEFAULT 1,
    CreatedAt       DATETIME                NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Login UNIQUE (Login),
    CONSTRAINT CK_Users_Role CHECK ([Role] IN (N'Администратор', N'Диспетчер'))
);
GO

/* ============================================================
   2. Таблица Clients — Клиенты
   ============================================================ */
CREATE TABLE Clients
(
    ClientId        INT IDENTITY(1,1)       NOT NULL,
    ClientType      NVARCHAR(20)            NOT NULL,  -- 'Юр. лицо' / 'Физ. лицо'
    Name            NVARCHAR(200)           NOT NULL,
    ContactPerson   NVARCHAR(150)           NULL,
    Phone           NVARCHAR(30)            NULL,
    Email           NVARCHAR(100)           NULL,
    [Address]       NVARCHAR(250)           NULL,
    ContractNumber  NVARCHAR(50)            NULL,
    ContractDate    DATE                    NULL,
    Notes           NVARCHAR(MAX)           NULL,
    CreatedAt       DATETIME                NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Clients PRIMARY KEY (ClientId),
    CONSTRAINT CK_Clients_ClientType CHECK (ClientType IN (N'Юр. лицо', N'Физ. лицо'))
);
GO

CREATE INDEX IX_Clients_Name ON Clients(Name);
GO

/* ============================================================
   3. Таблица Transport — Транспорт
   ============================================================ */
CREATE TABLE Transport
(
    TransportId         INT IDENTITY(1,1)       NOT NULL,
    VehicleNumber       NVARCHAR(20)            NOT NULL,
    Model               NVARCHAR(100)           NOT NULL,
    BodyType            NVARCHAR(100)           NOT NULL,
    CapacityTons        DECIMAL(10,2)           NOT NULL,
    FuelConsumption     DECIMAL(10,2)           NOT NULL,  -- л/100 км
    CostPerKm           DECIMAL(10,2)           NOT NULL,
    IdleHourCost        DECIMAL(10,2)           NOT NULL,
    [Status]            NVARCHAR(30)            NOT NULL,  -- 'Свободен' / 'В рейсе' / 'На ремонте'
    Notes               NVARCHAR(MAX)           NULL,
    CreatedAt           DATETIME                NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Transport PRIMARY KEY (TransportId),
    CONSTRAINT UQ_Transport_VehicleNumber UNIQUE (VehicleNumber),
    CONSTRAINT CK_Transport_Status CHECK ([Status] IN (N'Свободен', N'В рейсе', N'На ремонте'))
);
GO

CREATE INDEX IX_Transport_Status ON Transport([Status]);
GO

/* ============================================================
   4. Таблица Tariffs — Тарифы
   ============================================================ */
CREATE TABLE Tariffs
(
    TariffId            INT IDENTITY(1,1)       NOT NULL,
    TariffName          NVARCHAR(100)           NOT NULL,
    CalculationType     NVARCHAR(30)            NOT NULL,  -- 'За км' / 'За час' / 'За тонну' / 'Смешанный'
    CostPerKm           DECIMAL(10,2)           NULL,
    CostPerHour         DECIMAL(10,2)           NULL,
    CostPerTon          DECIMAL(10,2)           NULL,
    FuelSurcharge       DECIMAL(10,2)           NULL,
    SeasonalCoefficient DECIMAL(10,2)           NOT NULL DEFAULT 1.00,
    IsActive            BIT                     NOT NULL DEFAULT 1,
    Notes               NVARCHAR(MAX)           NULL,
    CreatedAt           DATETIME                NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Tariffs PRIMARY KEY (TariffId),
    CONSTRAINT CK_Tariffs_CalcType CHECK (CalculationType IN (N'За км', N'За час', N'За тонну', N'Смешанный'))
);
GO

/* ============================================================
   5. Таблица GeoPoints — Геоточки (города/адреса)
   ============================================================ */
CREATE TABLE GeoPoints
(
    GeoPointId      INT IDENTITY(1,1)       NOT NULL,
    PointName       NVARCHAR(150)           NOT NULL,
    Region          NVARCHAR(100)           NULL,
    Latitude        DECIMAL(10,6)           NULL,
    Longitude       DECIMAL(10,6)           NULL,
    Notes           NVARCHAR(MAX)           NULL,

    CONSTRAINT PK_GeoPoints PRIMARY KEY (GeoPointId)
);
GO

CREATE INDEX IX_GeoPoints_PointName ON GeoPoints(PointName);
GO

/* ============================================================
   6. Таблица Distances — Расстояния между точками
   ============================================================ */
CREATE TABLE Distances
(
    DistanceId      INT IDENTITY(1,1)       NOT NULL,
    PointFromId     INT                     NOT NULL,
    PointToId       INT                     NOT NULL,
    DistanceKm      DECIMAL(10,2)           NOT NULL,

    CONSTRAINT PK_Distances PRIMARY KEY (DistanceId),
    CONSTRAINT FK_Distances_PointFrom FOREIGN KEY (PointFromId) REFERENCES GeoPoints(GeoPointId),
    CONSTRAINT FK_Distances_PointTo   FOREIGN KEY (PointToId)   REFERENCES GeoPoints(GeoPointId),
    CONSTRAINT CK_Distances_Different CHECK (PointFromId <> PointToId)
);
GO

-- Уникальная пара (откуда, куда) — без дублей
CREATE UNIQUE INDEX UQ_Distances_Points ON Distances(PointFromId, PointToId);
GO

/* ============================================================
   7. Таблица AdditionalServices — Дополнительные услуги
   ============================================================ */
CREATE TABLE AdditionalServices
(
    ServiceId       INT IDENTITY(1,1)       NOT NULL,
    ServiceName     NVARCHAR(150)           NOT NULL,
    Price           DECIMAL(10,2)           NOT NULL,
    ChargeType      NVARCHAR(30)            NOT NULL,  -- 'Фиксированная' / 'За единицу'
    UnitName        NVARCHAR(50)            NULL,
    IsActive        BIT                     NOT NULL DEFAULT 1,

    CONSTRAINT PK_AdditionalServices PRIMARY KEY (ServiceId),
    CONSTRAINT CK_Services_ChargeType CHECK (ChargeType IN (N'Фиксированная', N'За единицу'))
);
GO

/* ============================================================
   8. Таблица Orders — Заказы
   ============================================================ */
CREATE TABLE Orders
(
    OrderId                 INT IDENTITY(1,1)       NOT NULL,
    OrderNumber             NVARCHAR(30)            NOT NULL,
    OrderDate               DATETIME                NOT NULL DEFAULT GETDATE(),
    ClientId                INT                     NOT NULL,
    PointFromId             INT                     NOT NULL,
    PointToId               INT                     NOT NULL,
    TransportId             INT                     NOT NULL,
    TariffId                INT                     NOT NULL,
    DistanceKm              DECIMAL(10,2)           NOT NULL,
    CargoWeight             DECIMAL(10,2)           NULL,
    IdleHours               DECIMAL(10,2)           NULL,
    CalculatedAmount        DECIMAL(12,2)           NOT NULL,
    FinalAmount             DECIMAL(12,2)           NOT NULL,
    ManualAdjustmentReason  NVARCHAR(500)           NULL,
    [Status]                NVARCHAR(30)            NOT NULL,  -- 'Новый' / 'Подтвержден' / 'В пути' / 'Завершен' / 'Отменен'
    Notes                   NVARCHAR(MAX)           NULL,
    CreatedAt               DATETIME                NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
    CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber),
    CONSTRAINT FK_Orders_Client       FOREIGN KEY (ClientId)    REFERENCES Clients(ClientId),
    CONSTRAINT FK_Orders_PointFrom    FOREIGN KEY (PointFromId) REFERENCES GeoPoints(GeoPointId),
    CONSTRAINT FK_Orders_PointTo      FOREIGN KEY (PointToId)   REFERENCES GeoPoints(GeoPointId),
    CONSTRAINT FK_Orders_Transport    FOREIGN KEY (TransportId) REFERENCES Transport(TransportId),
    CONSTRAINT FK_Orders_Tariff       FOREIGN KEY (TariffId)    REFERENCES Tariffs(TariffId),
    CONSTRAINT CK_Orders_Status CHECK ([Status] IN (N'Новый', N'Подтвержден', N'В пути', N'Завершен', N'Отменен'))
);
GO

CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
CREATE INDEX IX_Orders_ClientId ON Orders(ClientId);
CREATE INDEX IX_Orders_Status ON Orders([Status]);
GO

/* ============================================================
   9. Таблица OrderServices — Связь заказов и доп. услуг
   ============================================================ */
CREATE TABLE OrderServices
(
    OrderServiceId  INT IDENTITY(1,1)       NOT NULL,
    OrderId         INT                     NOT NULL,
    ServiceId       INT                     NOT NULL,
    Quantity        DECIMAL(10,2)           NOT NULL DEFAULT 1,
    Price           DECIMAL(10,2)           NOT NULL,
    Total           DECIMAL(12,2)           NOT NULL,

    CONSTRAINT PK_OrderServices PRIMARY KEY (OrderServiceId),
    CONSTRAINT FK_OrderServices_Order   FOREIGN KEY (OrderId)   REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderServices_Service FOREIGN KEY (ServiceId) REFERENCES AdditionalServices(ServiceId)
);
GO

/* ============================================================
   10. Таблица OrderStatusHistory — История изменения статусов
   ============================================================ */
CREATE TABLE OrderStatusHistory
(
    HistoryId           INT IDENTITY(1,1)       NOT NULL,
    OrderId             INT                     NOT NULL,
    OldStatus           NVARCHAR(30)            NULL,
    NewStatus           NVARCHAR(30)            NOT NULL,
    ChangedAt           DATETIME                NOT NULL DEFAULT GETDATE(),
    ChangedByUserId     INT                     NULL,
    Comment             NVARCHAR(250)           NULL,

    CONSTRAINT PK_OrderStatusHistory PRIMARY KEY (HistoryId),
    CONSTRAINT FK_StatusHistory_Order   FOREIGN KEY (OrderId)         REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_StatusHistory_User    FOREIGN KEY (ChangedByUserId) REFERENCES Users(UserId)
);
GO

CREATE INDEX IX_StatusHistory_OrderId ON OrderStatusHistory(OrderId);
GO

PRINT 'Все таблицы созданы успешно.';
GO

/* ============================================================
   ТЕСТОВЫЕ ДАННЫЕ
   ============================================================ */

-- ============================================================
-- Пользователи
-- Пароли хранятся в открытом виде для учебного проекта
-- ============================================================
INSERT INTO Users (Login, PasswordHash, FullName, [Role], IsActive) VALUES
(N'admin',      N'admin123',  N'Администратор Системы',  N'Администратор', 1),
(N'dispatcher', N'disp123',   N'Диспетчер Иванов',       N'Диспетчер',     1),
(N'disp2',      N'disp456',   N'Диспетчер Петрова',      N'Диспетчер',     1);
GO

-- ============================================================
-- Клиенты
-- ============================================================
INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, [Address], ContractNumber, ContractDate, Notes) VALUES
(N'Юр. лицо', N'ООО "ТрансЛогистик"',      N'Сидоров Алексей Петрович',  N'+7 (495) 111-22-33', N'sidorov@translog.ru',   N'г. Москва, ул. Складская, д. 15',     N'ДГ-2024-001', '2024-01-15', N'Постоянный клиент, крупные партии'),
(N'Юр. лицо', N'ИП Козлов М.В.',            N'Козлов Михаил Викторович', N'+7 (812) 444-55-66', N'kozlov@mail.ru',        N'г. Санкт-Петербург, пр. Невский, 100', N'ДГ-2024-002', '2024-02-20', NULL),
(N'Физ. лицо',N'Смирнова Елена Николаевна', N'Смирнова Елена Николаевна',N'+7 (903) 777-88-99', N'smirnova.el@gmail.com', N'г. Казань, ул. Баумана, д. 25, кв. 8', NULL,          NULL,        N'Доставка личных вещей'),
(N'Юр. лицо', N'ООО "СтройМонтаж"',         N'Кузнецов Дмитрий Олегович',N'+7 (343) 100-20-30', N'kuznetsov@stroymont.ru',N'г. Екатеринбург, ул. Ленина, д. 50',   N'ДГ-2024-003', '2024-03-10', N'Строительные материалы'),
(N'Юр. лицо', N'АО "ПродТорг"',             N'Новикова Анна Сергеевна',  N'+7 (913) 555-12-34', N'novikova@prodtorg.ru',  N'г. Новосибирск, Красный пр., д. 80',   N'ДГ-2024-004', '2024-04-01', N'Продуктовая продукция, рефрижератор');
GO

-- ============================================================
-- Транспорт
-- ============================================================
INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, [Status], Notes) VALUES
(N'А123ВС 77',  N'КАМАЗ 65115',      N'Тент',         10.00,  25.00,  35.00,  500.00, N'Свободен',    N'Основной тягач'),
(N'В456КЕ 78',  N'MAN TGX 18.510',   N'Рефрижератор', 20.00,  32.00,  55.00,  800.00, N'Свободен',    N'Для скоропортящихся грузов'),
(N'Е789НО 16',  N'ГАЗель NEXT',      N'Фургон',        1.50,  10.00,  15.00,  300.00, N'В рейсе',     N'Малотоннажный, по городу'),
(N'К012РС 52',  N'Volvo FH16',       N'Тент',         22.00,  35.00,  60.00,  900.00, N'Свободен',    N'Дальнобойный, Европа-Россия'),
(N'М345ТУ 23',  N'МАЗ 5337',         N'Бортовой',      8.00,   22.00,  28.00,  450.00, N'На ремонте',  N'Плановое ТО до 20.04.2026'),
(N'О678ХЦ 96',  N'Scania R450',      N'Контейнеровоз', 30.00,  38.00,  70.00,  1000.00,N'Свободен',   N'Контейнерные перевозки 20/40 футов');
GO

-- ============================================================
-- Тарифы
-- ============================================================
INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) VALUES
(N'Стандартный',      N'Смешанный',   35.00,  500.00,  200.00,  500.00,  1.00, 1, N'Базовый тариф для большинства перевозок'),
(N'Экспресс',         N'За км',       55.00,  700.00,  NULL,    800.00,  1.15, 1, N'Срочная доставка, повышенный коэффициент'),
(N'Эконом',           N'За тонну',    NULL,   NULL,    150.00,  300.00,  1.00, 1, N'Для крупных партий, только по весу'),
(N'Рефрижератор',     N'Смешанный',   50.00,  600.00,  300.00,  700.00,  1.10, 1, N'Перевозка скоропортящихся продуктов'),
(N'Сезонный летний',  N'Смешанный',   40.00,  550.00,  250.00,  600.00,  1.20, 0, N'Летний сезонный тариф (июнь-август)');
GO

-- ============================================================
-- Геоточки
-- ============================================================
INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES
(N'Москва',           N'Московская обл.',        55.7558,  37.6173,  N'Столица, основной хаб'),
(N'Санкт-Петербург',  N'Ленинградская обл.',     59.9343,  30.3351,  N'Северо-Западный хаб'),
(N'Казань',           N'Республика Татарстан',   55.7879,  49.1233,  N'Поволжье'),
(N'Екатеринбург',     N'Свердловская обл.',      56.8389,  60.6057,  N'Урал'),
(N'Новосибирск',      N'Новосибирская обл.',     55.0084,  82.9357,  N'Сибирь'),
(N'Нижний Новгород',  N'Нижегородская обл.',     56.2965,  43.9361,  N'Поволжье'),
(N'Ростов-на-Дону',   N'Ростовская обл.',        47.2357,  39.7015,  N'Юг России'),
(N'Самара',           N'Самарская обл.',         53.1959,  50.1002,  N'Поволжье');
GO

-- ============================================================
-- Расстояния (км)
-- ============================================================
INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES
(1, 2, 710),    -- Москва → Санкт-Петербург
(2, 1, 710),    -- Санкт-Петербург → Москва
(1, 3, 820),    -- Москва → Казань
(3, 1, 820),    -- Казань → Москва
(1, 4, 1780),   -- Москва → Екатеринбург
(4, 1, 1780),   -- Екатеринбург → Москва
(1, 5, 3360),   -- Москва → Новосибирск
(5, 1, 3360),   -- Новосибирск → Москва
(1, 6, 420),    -- Москва → Нижний Новгород
(6, 1, 420),    -- Нижний Новгород → Москва
(1, 7, 1080),   -- Москва → Ростов-на-Дону
(7, 1, 1080),   -- Ростов-на-Дону → Москва
(1, 8, 1060),   -- Москва → Самара
(8, 1, 1060),   -- Самара → Москва
(2, 3, 1530),   -- СПб → Казань
(3, 2, 1530),   -- Казань → СПб
(2, 6, 1130),   -- СПб → Нижний Новгород
(6, 2, 1130),   -- Нижний Новгород → СПб
(3, 4, 1000),   -- Казань → Екатеринбург
(4, 3, 1000),   -- Екатеринбург → Казань
(4, 5, 1600),   -- Екатеринбург → Новосибирск
(5, 4, 1600),   -- Новосибирск → Екатеринбург
(6, 8, 770),    -- Нижний Новгород → Самара
(8, 6, 770),    -- Самара → Нижний Новгород
(7, 8, 1070),   -- Ростов → Самара
(8, 7, 1070);   -- Самара → Ростов
GO

-- ============================================================
-- Дополнительные услуги
-- ============================================================
INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES
(N'Погрузка',           3000.00,  N'Фиксированная', N'услуга',   1),
(N'Разгрузка',          3000.00,  N'Фиксированная', N'услуга',   1),
(N'Экспедирование',     1500.00,  N'Фиксированная', N'услуга',   1),
(N'Срочная доставка',   5000.00,  N'Фиксированная', N'услуга',   1),
(N'Страхование груза',  2000.00,  N'Фиксированная', N'услуга',   1),
(N'Такелажные работы',  4500.00,  N'За единицу',    N'час',      1),
(N'Хранение на складе', 500.00,   N'За единицу',    N'сутки',    1),
(N'Обрешётка',         1200.00,   N'За единицу',    N'место',    1);
GO

-- ============================================================
-- Заказы (тестовые)
-- ============================================================

-- Заказ 1: Москва → Санкт-Петербург
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0001', '2026-04-01', 1, 1, 2, 1, 1, 710, 8.00, 2.00,
        -- Расчёт: 710*35 + 2*500 + 8*200 + 500 = 24850 + 1000 + 1600 + 500 = 27950 * 1.00 = 27950.00
        27950.00, 27950.00, NULL, N'Завершен', N'Доставка стройматериалов');

-- Заказ 2: Москва → Казань
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0002', '2026-04-03', 2, 1, 3, 4, 1, 820, 15.00, 4.00,
        -- 820*35 + 4*500 + 15*200 + 500 = 28700+2000+3000+500 = 34200 * 1.00 = 34200.00
        34200.00, 34200.00, NULL, N'В пути', N'Продукты питания');

-- Заказ 3: Москва → Екатеринбург (рефрижератор)
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0003', '2026-04-05', 5, 1, 4, 2, 4, 1780, 18.00, 3.00,
        -- 1780*50 + 3*600 + 18*300 + 700 = 89000+1800+5400+700 = 96900 * 1.10 = 106590.00
        96900.00, 106590.00, NULL, N'Подтвержден', N'Молочная продукция');

-- Заказ 4: Москва → Новосибирск
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0004', '2026-04-07', 3, 1, 5, 1, 3, 3360, 1.20, 0.00,
        -- 3360*NULL + 0*NULL + 1.2*150 + 300 = 0+0+180+300 = 480 * 1.00 = 480.00 (эконом по тоннажу)
        -- Пересчитаем: CostPerKm=NULL, CostPerHour=NULL, CostPerTon=150, FuelSurcharge=300
        -- 0 + 0 + 1.2*150 + 300 = 480
        480.00, 480.00, NULL, N'Новый', N'Личные вещи переезд');

-- Заказ 5: Москва → Нижний Новгород
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0005', '2026-04-08', 4, 1, 6, 1, 1, 420, 9.00, 1.00,
        -- 420*35 + 1*500 + 9*200 + 500 = 14700+500+1800+500 = 17500 * 1.00 = 17500.00
        17500.00, 17500.00, NULL, N'Новый', N'Строительное оборудование');

-- Заказ 6: Санкт-Петербург → Казань (отменён)
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0006', '2026-04-02', 1, 2, 3, 4, 2, 1530, 0.00, 0.00,
        -- 1530*55 + 0*700 + 0*NULL + 800 = 84150+0+0+800 = 84950 * 1.15 = 97692.50
        84950.00, 97692.50, N'Скидка 10% за объём', 97692.50, N'Отменен', N'Клиент отказался');

-- Заказ 7: Екатеринбург → Новосибирск
INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, [Status], Notes)
VALUES (N'ЗК-2026-0007', '2026-04-10', 2, 4, 5, 4, 1, 1600, 20.00, 5.00,
        -- 1600*35 + 5*500 + 20*200 + 500 = 56000+2500+4000+500 = 63000 * 1.00 = 63000.00
        63000.00, 63000.00, NULL, N'Новый', N'Оборудование для производства');
GO

-- ============================================================
-- OrderServices — Связи заказов с доп. услугами
-- ============================================================
INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES
-- Заказ 1: погрузка + разгрузка
(1, 1, 1, 3000.00, 3000.00),
(1, 2, 1, 3000.00, 3000.00),
-- Заказ 2: погрузка + экспедирование
(2, 1, 1, 3000.00, 3000.00),
(2, 3, 1, 1500.00, 1500.00),
-- Заказ 3: погрузка + разгрузка + страхование
(3, 1, 1, 3000.00, 3000.00),
(3, 2, 1, 3000.00, 3000.00),
(3, 5, 1, 2000.00, 2000.00),
-- Заказ 5: погрузка
(5, 1, 1, 3000.00, 3000.00);
GO

-- ============================================================
-- OrderStatusHistory — История изменения статусов
-- ============================================================
INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES
-- Заказ 1: полный цикл
(1, NULL,          N'Новый',       '2026-04-01 09:00', 1, N'Заказ создан'),
(1, N'Новый',      N'Подтвержден', '2026-04-01 10:30', 1, N'Подтверждён клиентом'),
(1, N'Подтвержден',N'В пути',      '2026-04-01 14:00', 2, N'Машина выехала'),
(1, N'В пути',     N'Завершен',    '2026-04-02 08:00', 2, N'Груз доставлен'),
-- Заказ 2
(2, NULL,          N'Новый',       '2026-04-03 09:00', 1, N'Заказ создан'),
(2, N'Новый',      N'Подтвержден', '2026-04-03 11:00', 2, N'Подтверждён'),
(2, N'Подтвержден',N'В пути',      '2026-04-04 07:00', 2, N'В пути'),
-- Заказ 3
(3, NULL,          N'Новый',       '2026-04-05 08:00', 1, N'Заказ создан'),
(3, N'Новый',      N'Подтвержден', '2026-04-05 09:30', 1, N'Рефрижератор подтверждён'),
-- Заказ 6: отменён
(6, NULL,          N'Новый',       '2026-04-02 10:00', 2, N'Заказ создан'),
(6, N'Новый',      N'Отменен',     '2026-04-02 15:00', 1, N'Клиент отказался');
GO

PRINT 'Тестовые данные добавлены успешно.';
PRINT '==========================================';
PRINT 'База данных DispatchLogisticsDB полностью создана и заполнена.';
PRINT 'Пользователи: admin/admin123, dispatcher/disp123, disp2/disp456';
PRINT '==========================================';
