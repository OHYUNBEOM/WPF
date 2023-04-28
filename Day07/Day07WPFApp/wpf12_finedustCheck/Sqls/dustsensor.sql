SELECT * FROM dustsensor;CREATE TABLE `dustsensor` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Dev_id` varchar(45) DEFAULT NULL,
  `Name` varchar(20) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `Loc` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `Coordx` double DEFAULT NULL,
  `Coordy` double DEFAULT NULL,
  `Ison` bit(1) DEFAULT NULL,
  `Pm10_after` int DEFAULT NULL,
  `Pm25_after` int DEFAULT NULL,
  `State` int DEFAULT NULL,
  `Timestamp` datetime DEFAULT NULL,
  `Company_id` varchar(50) DEFAULT NULL,
  `Company_name` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=47 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
