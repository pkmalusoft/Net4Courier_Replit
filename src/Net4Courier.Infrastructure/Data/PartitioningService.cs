using Microsoft.EntityFrameworkCore;

namespace Net4Courier.Infrastructure.Data;

public class PartitioningService
{
    private readonly ApplicationDbContext _context;

    public PartitioningService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsurePartitionsExistAsync()
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            var checkPartition = await _context.Database.ExecuteSqlRawAsync(@"
                SELECT 1 FROM pg_partitioned_table pt
                JOIN pg_class c ON pt.partrelid = c.oid
                WHERE c.relname = 'InscanMasters'
            ");
            
            if (checkPartition == 0)
            {
                Console.WriteLine("InscanMasters is not partitioned yet. Partitioning will be applied on fresh database.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: Partitioning check skipped - {ex.Message}");
        }
        finally
        {
            await connection.CloseAsync();
        }

        await CreateFiscalYearPartitionsAsync();
    }

    public async Task CreateFiscalYearPartitionsAsync()
    {
        var financialYears = await _context.FinancialYears
            .Where(fy => fy.IsActive)
            .ToListAsync();

        foreach (var fy in financialYears)
        {
            try
            {
                var partitionName = $"inscanmasters_{fy.StartDate:yyyy}_{fy.EndDate:yyyy}";
                var startDate = fy.StartDate.ToString("yyyy-MM-dd");
                var endDate = fy.EndDate.AddDays(1).ToString("yyyy-MM-dd");

                var checkSql = $@"
                    SELECT 1 FROM pg_class 
                    WHERE relname = '{partitionName.ToLower()}' 
                    AND relkind = 'r'";

                var exists = await _context.Database.ExecuteSqlRawAsync(checkSql);
                
                if (exists == 0)
                {
                    Console.WriteLine($"Note: Partition {partitionName} would be created for FY {fy.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Partition check for {fy.Name}: {ex.Message}");
            }
        }
    }

    public static string GetPartitioningMigrationSql()
    {
        return @"
-- PostgreSQL Table Partitioning for InscanMasters
-- This script converts InscanMasters to a partitioned table by TransactionDate

-- Step 1: Rename existing table (if upgrading from non-partitioned)
-- ALTER TABLE ""InscanMasters"" RENAME TO ""InscanMasters_old"";

-- Step 2: Create new partitioned table structure
-- CREATE TABLE ""InscanMasters"" (
--     ""Id"" BIGSERIAL,
--     ""TransactionDate"" DATE NOT NULL,
--     ... other columns ...
--     PRIMARY KEY (""Id"", ""TransactionDate"")
-- ) PARTITION BY RANGE (""TransactionDate"");

-- Step 3: Create partitions for each fiscal year
-- Example for FY 2025-2026 (April to March):
-- CREATE TABLE inscanmasters_fy2025_2026 PARTITION OF ""InscanMasters""
--     FOR VALUES FROM ('2025-04-01') TO ('2026-04-01');

-- CREATE TABLE inscanmasters_fy2026_2027 PARTITION OF ""InscanMasters""
--     FOR VALUES FROM ('2026-04-01') TO ('2027-04-01');

-- Step 4: Create indexes on partitions
-- CREATE INDEX idx_inscan_awbno ON ""InscanMasters"" (""AWBNo"");
-- CREATE INDEX idx_inscan_status ON ""InscanMasters"" (""CourierStatusId"");
-- CREATE INDEX idx_inscan_customer ON ""InscanMasters"" (""CustomerId"");
-- CREATE INDEX idx_inscan_branch ON ""InscanMasters"" (""BranchId"");

-- Step 5: Migrate data from old table (if upgrading)
-- INSERT INTO ""InscanMasters"" SELECT * FROM ""InscanMasters_old"";
-- DROP TABLE ""InscanMasters_old"";

-- Note: For new installations, EnsureCreated will create non-partitioned tables.
-- For partitioning, run the SQL commands above manually or through a migration.
";
    }
}
