-- Create Database and User
CREATE DATABASE `orders` /*!40100 COLLATE 'latin1_swedish_ci' */;

-- Create User
CREATE USER 'order-service'@'%' IDENTIFIED BY 'nmCsdkhj20n@Sa';
GRANT ALL PRIVILEGES ON *.* TO 'order-service'@'%' IDENTIFIED BY 'nmCsdkhj20n@Sa' WITH GRANT OPTION;

USE `orders`;

-- Create Functions
DELIMITER //
CREATE FUNCTION `randomGuid`() RETURNS binary(16)
BEGIN
RETURN (UNHEX(REPLACE(MD5(UUID()), '-', '')));
END//

CREATE FUNCTION `binToGuid`(`value` BINARY(16)) RETURNS char(36)
BEGIN
SET @guidString = HEX(value);
RETURN LOWER(
CONCAT(
	SUBSTR(@guidString,7,2),
	SUBSTR(@guidString,5,2),
	SUBSTR(@guidString,3,2),
	SUBSTR(@guidString,1,2),
	'-',
	SUBSTR(@guidString,11,2),
	SUBSTR(@guidString,9,2),
	'-',
	SUBSTR(@guidString,15,2),
	SUBSTR(@guidString,13,2),
	'-',
	SUBSTR(@guidString,17,4),
	'-',
	SUBSTR(@guidString,21,12)
));
END//

CREATE FUNCTION `guidToBin`(`guidString` CHAR(36)) RETURNS binary(16)
BEGIN
SET @str = CONCAT(
	SUBSTR(guidString,7,2),
	SUBSTR(guidString,5,2),
	SUBSTR(guidString,3,2),
	SUBSTR(guidString,1,2),
	SUBSTR(guidString,12,2),
	SUBSTR(guidString,10,2),
	SUBSTR(guidString,17,2),
	SUBSTR(guidString,15,2),
	SUBSTR(guidString,20,4),
	SUBSTR(guidString,25,12)
);

RETURN UNHEX(@str);
END//

DELIMITER ;

-- Create Tables
CREATE TABLE IF NOT EXISTS `order_status` (
  `Id` binary(16) NOT NULL,
  `Name` varchar(20) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `Status` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `order_service` (
  `Id` binary(16) NOT NULL,
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `order_product` (
  `Id` binary(16) NOT NULL,
  `ServiceId` binary(16) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `UnitCost` decimal(4,2) NOT NULL,
  `UnitPrice` decimal(4,2) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `order_service_opfk_1` FOREIGN KEY (`ServiceId`) REFERENCES `order_service` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `order` (
  `Id` binary(16) NOT NULL,
  `ResellerId` binary(16) NOT NULL,
  `CustomerId` binary(16) NOT NULL,
  `StatusId` binary(16) NOT NULL,
  `CreatedDate` datetime NOT NULL,  
  PRIMARY KEY (`Id`),
  KEY `StatusId` (`StatusId`),
  KEY `CustomerId` (`CustomerId`),
  CONSTRAINT `order_ofk_1` FOREIGN KEY (`StatusId`) REFERENCES `order_status` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `order_item` (
  `Id` binary(16) NOT NULL,
  `OrderId` binary(16) NOT NULL,
  `ProductId` binary(16) NOT NULL,
  `ServiceId` binary(16) NOT NULL,
  `Quantity` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `OrderId` (`OrderId`),
  KEY `ProductId` (`ProductId`),
  KEY `ServiceId` (`ServiceId`),
  CONSTRAINT `order_item_oifk_1` FOREIGN KEY (`OrderId`) REFERENCES `order` (`Id`),
  CONSTRAINT `order_service_oifk_1` FOREIGN KEY (`ServiceId`) REFERENCES `order_service` (`Id`),
  CONSTRAINT `order_product_oifk_1` FOREIGN KEY (`ProductId`) REFERENCES `order_product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Create variables
SET @OrderStatusCreated = randomGuid();
SET @OrderStatusInProgress = randomGuid();
SET @OrderStatusFailed = randomGuid();
SET @OrderStatusCompleted = randomGuid();

SET @ResellerId1 = randomGuid();
SET @ResellerId2 = randomGuid();
SET @ResellerId3 = randomGuid();
SET @ResellerId4 = randomGuid();
SET @ResellerId5 = randomGuid();
SET @ResellerId6 = randomGuid();
SET @ResellerId7 = randomGuid();
SET @ResellerId8 = randomGuid();
SET @ResellerId9 = randomGuid();

SET @ServiceIdA = randomGuid();
SET @ServiceIdB = randomGuid();
SET @ServiceIdC = randomGuid();

SET @ProductIdA1 = randomGuid();
SET @ProductIdA2 = randomGuid();
SET @ProductIdA3 = randomGuid();
SET @ProductIdB1 = randomGuid();
SET @ProductIdB2 = randomGuid();
SET @ProductIdB3 = randomGuid();
SET @ProductIdC1 = randomGuid();
SET @ProductIdC2 = randomGuid();
SET @ProductIdC3 = randomGuid();

SET @MaxUnitCost = 10;
SET @Margin = 1.15;
SET @ProductIdA1UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdA2UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdA3UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdB1UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdB2UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdB3UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdC1UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdC2UnitCost = 1 + RAND() * @MaxUnitCost;
SET @ProductIdC3UnitCost = 1 + RAND() * @MaxUnitCost;

-- Create Stored Procedures
DELIMITER //
CREATE PROCEDURE `InsertOrders`()
BEGIN

SET @OrdersToCreate = 100;
SET @MaxQuantity = 100;
SET @OrderNumber = 1;

WHILE @OrderNumber <= @OrdersToCreate DO

	-- Insert Order
	SET @RandomOrderId = randomGuid();
	SET @RandomResellerIndex = FLOOR(1 + RAND() * 9);
	SET @RandomResellerId = guidToBin(ELT(@RandomResellerIndex, binToGuid(@ResellerId1), binToGuid(@ResellerId2), binToGuid(@ResellerId3), binToGuid(@ResellerId4), binToGuid(@ResellerId5), binToGuid(@ResellerId6), binToGuid(@ResellerId7), binToGuid(@ResellerId8), binToGuid(@ResellerId9)));				
	SET @RandomOrderStatusIndex = FLOOR(1 + RAND() * 4);
	SET @RandomOrderStatusId = guidToBin(ELT(@RandomOrderStatusIndex, binToGuid(@OrderStatusCreated), binToGuid(@OrderStatusInProgress), binToGuid(@OrderStatusFailed), binToGuid(@OrderStatusCompleted)));		
	SET @RandomCreatedDate = FROM_UNIXTIME(UNIX_TIMESTAMP('2018-01-01 00:00:00') + FLOOR(0 + (RAND() * 63072000)));

	INSERT IGNORE INTO `order` (`Id`, `ResellerId`, `CustomerId`, `StatusId`, `CreatedDate`) VALUES
		(@RandomOrderId, @RandomResellerId, randomGuid(), @RandomOrderStatusId, @RandomCreatedDate);
		
	-- Insert Order Item(s)
	SET @ItemNumber = 1;
	SET @NumberOfItemsInOrder = FLOOR(1 + RAND() * 3);
	WHILE @ItemNumber <= @NumberOfItemsInOrder DO
		SET @RandomOrderItemId = randomGuid();
		SET @RandomServiceIndex = FLOOR(1 + RAND() * 3);
		SET @RandomServiceId = guidToBin(ELT(@RandomServiceIndex, binToGuid(@ServiceIdA), binToGuid(@ServiceIdB), binToGuid(@ServiceIdC)));			
		SET @RandomProductId = (SELECT Id FROM order_product WHERE ServiceId = @RandomServiceId ORDER BY RAND() LIMIT 1);
		SET @RandomQuantity = FLOOR(1 + RAND() * @MaxQuantity);
		
		INSERT IGNORE INTO `order_item` (`Id`, `OrderId`, `ProductId`, `ServiceId`, `Quantity`) VALUES
			(@RandomOrderItemId, @RandomOrderId, @RandomProductId, @RandomServiceId, @RandomQuantity);

		SET @ItemNumber = @ItemNumber + 1;
	END WHILE;	
		
	SET @OrderNumber = @OrderNumber + 1;

END WHILE;	
END//
DELIMITER ;

-- Insert Order Status
INSERT IGNORE INTO `order_status` (`Id`, `Name`) VALUES
	(@OrderStatusCreated, 'Created'),
	(@OrderStatusInProgress, 'In Progress'),
	(@OrderStatusFailed, 'Failed'),	
	(@OrderStatusCompleted, 'Completed');
	
-- Insert Services
INSERT IGNORE INTO `order_service` (`Id`, `Name`) VALUES
	(@ServiceIdA, 'Email'),
	(@ServiceIdB, 'Antivirus'),
	(@ServiceIdC, 'Backup');
	
-- Insert Products	
INSERT IGNORE INTO `order_product` (`Id`, `ServiceId`, `Name`, `UnitCost`, `UnitPrice` ) VALUES
	(@ProductIdA1, @ServiceIdA, '10GB Mailbox', @ProductIdA1UnitCost, @ProductIdA1UnitCost * @Margin),
	(@ProductIdA2, @ServiceIdA, '50GB Mailbox', @ProductIdA2UnitCost, @ProductIdA2UnitCost * @Margin),
	(@ProductIdA3, @ServiceIdA, '100GB Mailbox', @ProductIdA3UnitCost, @ProductIdA3UnitCost * @Margin),	
	(@ProductIdB1, @ServiceIdB, 'Basic Antivirus', @ProductIdB1UnitCost, @ProductIdB1UnitCost * @Margin),
	(@ProductIdB2, @ServiceIdB, 'Standard Antivirus', @ProductIdB2UnitCost, @ProductIdB2UnitCost * @Margin),
	(@ProductIdB3, @ServiceIdB, 'Premium Antivirus', @ProductIdB3UnitCost, @ProductIdB3UnitCost * @Margin),
	(@ProductIdC1, @ServiceIdC, '100GB Backup', @ProductIdC1UnitCost, @ProductIdC1UnitCost * @Margin),
	(@ProductIdC2, @ServiceIdC, '500GB Backup', @ProductIdC2UnitCost, @ProductIdC2UnitCost * @Margin),
	(@ProductIdC3, @ServiceIdC, '1TB Backup', @ProductIdC3UnitCost, @ProductIdC3UnitCost * @Margin);

-- Seed Orders
call InsertOrders();
	
	
	
