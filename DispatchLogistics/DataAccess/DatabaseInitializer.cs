using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DispatchLogistics.DataAccess
{
    /// <summary>
    /// Класс для автоматического создания базы данных и тестовых данных
    /// Вызывается при первом запуске приложения
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Проверяет существование БД и создаёт её если нужно
        /// </summary>
        public static void Initialize()
        {
            if (DatabaseHelper.DatabaseExists())
                return;

            CreateSchema();
            SeedData();
        }

        private static void CreateSchema()
        {
            using (SqliteConnection conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();

                // Users
                Execute(conn, @"
                    CREATE TABLE Users (
                        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Login TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        Role TEXT NOT NULL CHECK(Role IN ('Администратор', 'Диспетчер')),
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                    )");

                // Clients
                Execute(conn, @"
                    CREATE TABLE Clients (
                        ClientId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ClientType TEXT NOT NULL CHECK(ClientType IN ('Юр. лицо', 'Физ. лицо')),
                        Name TEXT NOT NULL,
                        ContactPerson TEXT,
                        Phone TEXT,
                        Email TEXT,
                        Address TEXT,
                        ContractNumber TEXT,
                        ContractDate TEXT,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                    )");
                Execute(conn, "CREATE INDEX IX_Clients_Name ON Clients(Name)");

                // Transport
                Execute(conn, @"
                    CREATE TABLE Transport (
                        TransportId INTEGER PRIMARY KEY AUTOINCREMENT,
                        VehicleNumber TEXT NOT NULL UNIQUE,
                        Model TEXT NOT NULL,
                        BodyType TEXT NOT NULL,
                        CapacityTons REAL NOT NULL,
                        FuelConsumption REAL NOT NULL,
                        CostPerKm REAL NOT NULL,
                        IdleHourCost REAL NOT NULL,
                        Status TEXT NOT NULL CHECK(Status IN ('Свободен', 'В рейсе', 'На ремонте')),
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                    )");
                Execute(conn, "CREATE INDEX IX_Transport_Status ON Transport(Status)");

                // Tariffs
                Execute(conn, @"
                    CREATE TABLE Tariffs (
                        TariffId INTEGER PRIMARY KEY AUTOINCREMENT,
                        TariffName TEXT NOT NULL,
                        CalculationType TEXT NOT NULL CHECK(CalculationType IN ('За км', 'За час', 'За тонну', 'Смешанный')),
                        CostPerKm REAL,
                        CostPerHour REAL,
                        CostPerTon REAL,
                        FuelSurcharge REAL,
                        SeasonalCoefficient REAL NOT NULL DEFAULT 1.00,
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                    )");

                // GeoPoints
                Execute(conn, @"
                    CREATE TABLE GeoPoints (
                        GeoPointId INTEGER PRIMARY KEY AUTOINCREMENT,
                        PointName TEXT NOT NULL,
                        Region TEXT,
                        Latitude REAL,
                        Longitude REAL,
                        Notes TEXT
                    )");
                Execute(conn, "CREATE INDEX IX_GeoPoints_PointName ON GeoPoints(PointName)");

                // Distances
                Execute(conn, @"
                    CREATE TABLE Distances (
                        DistanceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        PointFromId INTEGER NOT NULL REFERENCES GeoPoints(GeoPointId),
                        PointToId INTEGER NOT NULL REFERENCES GeoPoints(GeoPointId),
                        DistanceKm REAL NOT NULL,
                        CHECK(PointFromId <> PointToId)
                    )");
                Execute(conn, "CREATE UNIQUE INDEX UQ_Distances_Points ON Distances(PointFromId, PointToId)");

                // AdditionalServices
                Execute(conn, @"
                    CREATE TABLE AdditionalServices (
                        ServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ServiceName TEXT NOT NULL,
                        Price REAL NOT NULL,
                        ChargeType TEXT NOT NULL CHECK(ChargeType IN ('Фиксированная', 'За единицу')),
                        UnitName TEXT,
                        IsActive INTEGER NOT NULL DEFAULT 1
                    )");

                // Orders
                Execute(conn, @"
                    CREATE TABLE Orders (
                        OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderNumber TEXT NOT NULL UNIQUE,
                        OrderDate TEXT NOT NULL DEFAULT (datetime('now')),
                        ClientId INTEGER NOT NULL REFERENCES Clients(ClientId),
                        PointFromId INTEGER NOT NULL REFERENCES GeoPoints(GeoPointId),
                        PointToId INTEGER NOT NULL REFERENCES GeoPoints(GeoPointId),
                        TransportId INTEGER NOT NULL REFERENCES Transport(TransportId),
                        TariffId INTEGER NOT NULL REFERENCES Tariffs(TariffId),
                        DistanceKm REAL NOT NULL,
                        CargoWeight REAL,
                        IdleHours REAL,
                        CalculatedAmount REAL NOT NULL,
                        FinalAmount REAL NOT NULL,
                        ManualAdjustmentReason TEXT,
                        Status TEXT NOT NULL CHECK(Status IN ('Новый', 'Подтвержден', 'В пути', 'Завершен', 'Отменен')),
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                    )");
                Execute(conn, "CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate)");
                Execute(conn, "CREATE INDEX IX_Orders_ClientId ON Orders(ClientId)");
                Execute(conn, "CREATE INDEX IX_Orders_Status ON Orders(Status)");

                // OrderServices
                Execute(conn, @"
                    CREATE TABLE OrderServices (
                        OrderServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
                        ServiceId INTEGER NOT NULL REFERENCES AdditionalServices(ServiceId),
                        Quantity REAL NOT NULL DEFAULT 1,
                        Price REAL NOT NULL,
                        Total REAL NOT NULL
                    )");

                // OrderStatusHistory
                Execute(conn, @"
                    CREATE TABLE OrderStatusHistory (
                        HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
                        OldStatus TEXT,
                        NewStatus TEXT NOT NULL,
                        ChangedAt TEXT NOT NULL DEFAULT (datetime('now')),
                        ChangedByUserId INTEGER REFERENCES Users(UserId),
                        Comment TEXT
                    )");
                Execute(conn, "CREATE INDEX IX_StatusHistory_OrderId ON OrderStatusHistory(OrderId)");
            }
        }

        private static void SeedData()
        {
            using (SqliteConnection conn = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();

                // Users
                Execute(conn, "INSERT INTO Users (Login, PasswordHash, FullName, Role, IsActive) VALUES ('admin', 'admin123', 'Администратор Системы', 'Администратор', 1)");
                Execute(conn, "INSERT INTO Users (Login, PasswordHash, FullName, Role, IsActive) VALUES ('dispatcher', 'disp123', 'Диспетчер Иванов', 'Диспетчер', 1)");
                Execute(conn, "INSERT INTO Users (Login, PasswordHash, FullName, Role, IsActive) VALUES ('disp2', 'disp456', 'Диспетчер Петрова', 'Диспетчер', 1)");

                // Clients
                Execute(conn, "INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, Address, ContractNumber, ContractDate, Notes) VALUES ('Юр. лицо', 'ООО \"ТрансЛогистик\"', 'Сидоров Алексей Петрович', '+7 (495) 111-22-33', 'sidorov@translog.ru', 'г. Москва, ул. Складская, д. 15', 'ДГ-2024-001', '2024-01-15', 'Постоянный клиент, крупные партии')");
                Execute(conn, "INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, Address, ContractNumber, ContractDate, Notes) VALUES ('Юр. лицо', 'ИП Козлов М.В.', 'Козлов Михаил Викторович', '+7 (812) 444-55-66', 'kozlov@mail.ru', 'г. Санкт-Петербург, пр. Невский, 100', 'ДГ-2024-002', '2024-02-20', NULL)");
                Execute(conn, "INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, Address, ContractNumber, ContractDate, Notes) VALUES ('Физ. лицо', 'Смирнова Елена Николаевна', 'Смирнова Елена Николаевна', '+7 (903) 777-88-99', 'smirnova.el@gmail.com', 'г. Казань, ул. Баумана, д. 25, кв. 8', NULL, NULL, 'Доставка личных вещей')");
                Execute(conn, "INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, Address, ContractNumber, ContractDate, Notes) VALUES ('Юр. лицо', 'ООО \"СтройМонтаж\"', 'Кузнецов Дмитрий Олегович', '+7 (343) 100-20-30', 'kuznetsov@stroymont.ru', 'г. Екатеринбург, ул. Ленина, д. 50', 'ДГ-2024-003', '2024-03-10', 'Строительные материалы')");
                Execute(conn, "INSERT INTO Clients (ClientType, Name, ContactPerson, Phone, Email, Address, ContractNumber, ContractDate, Notes) VALUES ('Юр. лицо', 'АО \"ПродТорг\"', 'Новикова Анна Сергеевна', '+7 (913) 555-12-34', 'novikova@prodtorg.ru', 'г. Новосибирск, Красный пр., д. 80', 'ДГ-2024-004', '2024-04-01', 'Продуктовая продукция, рефрижератор')");

                // Transport
                Execute(conn, "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) VALUES ('А123ВС 77', 'КАМАЗ 65115', 'Тент', 10.00, 25.00, 35.00, 500.00, 'Свободен', 'Основной тягач')");
                Execute(conn, "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) VALUES ('В456КЕ 78', 'MAN TGX 18.510', 'Рефрижератор', 20.00, 32.00, 55.00, 800.00, 'Свободен', 'Для скоропортящихся грузов')");
                Execute(conn, "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) VALUES ('Е789НО 16', 'ГАЗель NEXT', 'Фургон', 1.50, 10.00, 15.00, 300.00, 'В рейсе', 'Малотоннажный, по городу')");
                Execute(conn, "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) VALUES ('К012РС 52', 'Volvo FH16', 'Тент', 22.00, 35.00, 60.00, 900.00, 'Свободен', 'Дальнобойный, Европа-Россия')");
                Execute(conn, "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) VALUES ('М345ТУ 23', 'МАЗ 5337', 'Бортовой', 8.00, 22.00, 28.00, 450.00, 'На ремонте', 'Плановое ТО до 20.04.2026')");
                Execute(conn, "INSERT INTO Transport (VehicleNumber, Model, BodyType, CapacityTons, FuelConsumption, CostPerKm, IdleHourCost, Status, Notes) VALUES ('О678ХЦ 96', 'Scania R450', 'Контейнеровоз', 30.00, 38.00, 70.00, 1000.00, 'Свободен', 'Контейнерные перевозки 20/40 футов')");

                // Tariffs
                Execute(conn, "INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) VALUES ('Стандартный', 'Смешанный', 35.00, 500.00, 200.00, 500.00, 1.00, 1, 'Базовый тариф для большинства перевозок')");
                Execute(conn, "INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) VALUES ('Экспресс', 'За км', 55.00, 700.00, NULL, 800.00, 1.15, 1, 'Срочная доставка, повышенный коэффициент')");
                Execute(conn, "INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) VALUES ('Эконом', 'За тонну', NULL, NULL, 150.00, 300.00, 1.00, 1, 'Для крупных партий, только по весу')");
                Execute(conn, "INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) VALUES ('Рефрижератор', 'Смешанный', 50.00, 600.00, 300.00, 700.00, 1.10, 1, 'Перевозка скоропортящихся продуктов')");
                Execute(conn, "INSERT INTO Tariffs (TariffName, CalculationType, CostPerKm, CostPerHour, CostPerTon, FuelSurcharge, SeasonalCoefficient, IsActive, Notes) VALUES ('Сезонный летний', 'Смешанный', 40.00, 550.00, 250.00, 600.00, 1.20, 0, 'Летний сезонный тариф (июнь-август)')");

                // GeoPoints
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Москва', 'Московская обл.', 55.7558, 37.6173, 'Столица, основной хаб')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Санкт-Петербург', 'Ленинградская обл.', 59.9343, 30.3351, 'Северо-Западный хаб')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Казань', 'Республика Татарстан', 55.7879, 49.1233, 'Поволжье')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Екатеринбург', 'Свердловская обл.', 56.8389, 60.6057, 'Урал')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Новосибирск', 'Новосибирская обл.', 55.0084, 82.9357, 'Сибирь')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Нижний Новгород', 'Нижегородская обл.', 56.2965, 43.9361, 'Поволжье')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Ростов-на-Дону', 'Ростовская обл.', 47.2357, 39.7015, 'Юг России')");
                Execute(conn, "INSERT INTO GeoPoints (PointName, Region, Latitude, Longitude, Notes) VALUES ('Самара', 'Самарская обл.', 53.1959, 50.1002, 'Поволжье')");

                // Distances (симметричные пары)
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 2, 710)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (2, 1, 710)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 3, 820)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (3, 1, 820)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 4, 1780)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (4, 1, 1780)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 5, 3360)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (5, 1, 3360)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 6, 420)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (6, 1, 420)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 7, 1080)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (7, 1, 1080)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (1, 8, 1060)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (8, 1, 1060)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (2, 3, 1530)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (3, 2, 1530)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (2, 6, 1130)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (6, 2, 1130)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (3, 4, 1000)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (4, 3, 1000)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (4, 5, 1600)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (5, 4, 1600)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (6, 8, 770)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (8, 6, 770)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (7, 8, 1070)");
                Execute(conn, "INSERT INTO Distances (PointFromId, PointToId, DistanceKm) VALUES (8, 7, 1070)");

                // AdditionalServices
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Погрузка', 3000.00, 'Фиксированная', 'услуга', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Разгрузка', 3000.00, 'Фиксированная', 'услуга', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Экспедирование', 1500.00, 'Фиксированная', 'услуга', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Срочная доставка', 5000.00, 'Фиксированная', 'услуга', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Страхование груза', 2000.00, 'Фиксированная', 'услуга', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Такелажные работы', 4500.00, 'За единицу', 'час', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Хранение на складе', 500.00, 'За единицу', 'сутки', 1)");
                Execute(conn, "INSERT INTO AdditionalServices (ServiceName, Price, ChargeType, UnitName, IsActive) VALUES ('Обрешётка', 1200.00, 'За единицу', 'место', 1)");

                // Orders (тестовые)
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0001', '2026-04-01', 1, 1, 2, 1, 1, 710, 8.00, 2.00, 27950.00, 27950.00, NULL, 'Завершен', 'Доставка стройматериалов')");
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0002', '2026-04-03', 2, 1, 3, 4, 1, 820, 15.00, 4.00, 34200.00, 34200.00, NULL, 'В пути', 'Продукты питания')");
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0003', '2026-04-05', 5, 1, 4, 2, 4, 1780, 18.00, 3.00, 96900.00, 106590.00, NULL, 'Подтвержден', 'Молочная продукция')");
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0004', '2026-04-07', 3, 1, 5, 1, 3, 3360, 1.20, 0.00, 480.00, 480.00, NULL, 'Новый', 'Личные вещи переезд')");
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0005', '2026-04-08', 4, 1, 6, 1, 1, 420, 9.00, 1.00, 17500.00, 17500.00, NULL, 'Новый', 'Строительное оборудование')");
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0006', '2026-04-02', 1, 2, 3, 4, 2, 1530, 0.00, 0.00, 84950.00, 97692.50, 'Скидка 10% за объём', 'Отменен', 'Клиент отказался')");
                Execute(conn, "INSERT INTO Orders (OrderNumber, OrderDate, ClientId, PointFromId, PointToId, TransportId, TariffId, DistanceKm, CargoWeight, IdleHours, CalculatedAmount, FinalAmount, ManualAdjustmentReason, Status, Notes) VALUES ('ЗК-2026-0007', '2026-04-10', 2, 4, 5, 4, 1, 1600, 20.00, 5.00, 63000.00, 63000.00, NULL, 'Новый', 'Оборудование для производства')");

                // OrderServices
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (1, 1, 1, 3000.00, 3000.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (1, 2, 1, 3000.00, 3000.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (2, 1, 1, 3000.00, 3000.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (2, 3, 1, 1500.00, 1500.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (3, 1, 1, 3000.00, 3000.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (3, 2, 1, 3000.00, 3000.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (3, 5, 1, 2000.00, 2000.00)");
                Execute(conn, "INSERT INTO OrderServices (OrderId, ServiceId, Quantity, Price, Total) VALUES (5, 1, 1, 3000.00, 3000.00)");

                // OrderStatusHistory
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (1, NULL, 'Новый', '2026-04-01 09:00', 1, 'Заказ создан')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (1, 'Новый', 'Подтвержден', '2026-04-01 10:30', 1, 'Подтверждён клиентом')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (1, 'Подтвержден', 'В пути', '2026-04-01 14:00', 2, 'Машина выехала')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (1, 'В пути', 'Завершен', '2026-04-02 08:00', 2, 'Груз доставлен')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (2, NULL, 'Новый', '2026-04-03 09:00', 1, 'Заказ создан')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (2, 'Новый', 'Подтвержден', '2026-04-03 11:00', 2, 'Подтверждён')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (2, 'Подтвержден', 'В пути', '2026-04-04 07:00', 2, 'В пути')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (3, NULL, 'Новый', '2026-04-05 08:00', 1, 'Заказ создан')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (3, 'Новый', 'Подтвержден', '2026-04-05 09:30', 1, 'Рефрижератор подтверждён')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (6, NULL, 'Новый', '2026-04-02 10:00', 2, 'Заказ создан')");
                Execute(conn, "INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Comment) VALUES (6, 'Новый', 'Отменен', '2026-04-02 15:00', 1, 'Клиент отказался')");
            }
        }

        private static void Execute(SqliteConnection conn, string sql)
        {
            using (SqliteCommand cmd = new SqliteCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
