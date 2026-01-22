-- Add PremiumSubscription column to Orders table
ALTER TABLE [ordering].[Orders]
ADD [PremiumSubscription] BIT NOT NULL DEFAULT 0;

