﻿using Dapper;
using EvAutoreg.Console.Data.Extensions;
using EvAutoreg.Console.Data.Models;
using EvAutoreg.Console.Data.SqlDataAccess;

namespace EvAutoreg.Console.Data.Data;

public class IssueData : IIssueData
{
    private readonly ISqlDataAccess _db;

    public IssueData(ISqlDataAccess db)
    {
        _db = db;
    }

    public void PrintIssue(XmlIssueModel xmlIssue)
    {
        System.Console.WriteLine(
            $"{xmlIssue.DateCreated}\n{xmlIssue.IssueNo}\n{xmlIssue.Author}\n{xmlIssue.Company}\n{xmlIssue.Status}\n{xmlIssue.Priority}\n{xmlIssue.Description}"
        );

        var issue = xmlIssue.ConvertToSqlModel();

        System.Console.ForegroundColor = ConsoleColor.DarkRed;
        System.Console.WriteLine($"New datetime: {issue.DateCreated}");
    }

    public async Task<IEnumerable<IssueModel>> GetAllIssues() =>
        await _db.LoadAll<IssueModel>("SELECT * FROM issue");

    public async Task<IssueModel?> GetIssue(string issueNo)
    {
        var results = await _db.LoadData<IssueModel, string>(
            "SELECT * FROM issue WHERE issue_no = @IssueNo",
            issueNo
        );

        return results.FirstOrDefault();
    }

    public async Task UpsertIssue(IssueModel issue)
    {
        var parameters = new DynamicParameters(issue);
        const string sql =
            @"INSERT INTO issue (issue_no, date_created, author, company,
                  status, priority, assigned_group, assignee, description) 
                  VALUES (@IssueNo, @DateCreated, @Author, @Company, @Status,
                  @Priority, @AssignedGroup, @Assignee, @Description)
                  ON CONFLICT (issue_no) DO UPDATE SET
                  author = EXCLUDED.author, company = EXCLUDED.company,
                  status = EXCLUDED.status, priority = EXCLUDED.priority,
                  assigned_group = EXCLUDED.assigned_group, assignee = EXCLUDED.assignee";

        await _db.SaveData(sql, parameters);
    }

    public async Task UpdateIssue(IssueModel issue)
    {
        var parameters = new DynamicParameters(issue);
        const string sql =
            @"UPDATE issue SET author = @Author, company = @Company,
                           status = @Status, priority = @Priority, assigned_group = @AssignedGroup,
                           assignee = @Assignee WHERE issue_no = @IssueNo";

        await _db.SaveData(sql, parameters);
    }
}
