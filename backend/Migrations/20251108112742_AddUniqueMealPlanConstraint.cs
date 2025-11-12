﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueMealPlanConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usuń duplikaty - zachowaj tylko najstarszy rekord dla każdej pary (UserId, Name, Date bez godziny)
            // Unikalność będzie wymuszana przez logikę aplikacji w MealPlanService
            migrationBuilder.Sql(@"
                DELETE FROM ""MealPlans"" a USING (
                    SELECT MIN(""Id"") as id, ""UserId"", ""Name"", (""Date""::date) as date_only
                    FROM ""MealPlans""
                    GROUP BY ""UserId"", ""Name"", (""Date""::date)
                    HAVING COUNT(*) > 1
                ) b
                WHERE a.""UserId"" = b.""UserId"" 
                  AND a.""Name"" = b.""Name"" 
                  AND (a.""Date""::date) = b.date_only
                  AND a.""Id"" != b.id;
            ");
            
            // Uwaga: Nie tworzymy unique index na wyrażeniu (Date::date) ponieważ
            // PostgreSQL wymaga funkcji IMMUTABLE, a rzutowanie timestamptz::date nie jest immutable.
            // Zamiast tego, unikalność jest wymuszana w warstwie aplikacji (MealPlanService).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Brak operacji - nie ma indeksu do usunięcia
        }
    }
}
