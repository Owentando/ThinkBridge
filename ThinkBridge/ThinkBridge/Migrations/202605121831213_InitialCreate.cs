namespace ThinkBridge.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AiSummary",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        Title = c.String(maxLength: 200),
                        OriginalText = c.String(nullable: false),
                        SummaryText = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.StudentId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Role = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatMessage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        SubjectId = c.Int(),
                        TopicId = c.Int(),
                        Message = c.String(nullable: false),
                        Response = c.String(nullable: false),
                        IsFromAi = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.StudentId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.StudentId)
                .Index(t => t.SubjectId)
                .Index(t => t.TopicId);
            
            CreateTable(
                "dbo.Subject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        LecturerId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LecturerId)
                .Index(t => t.LecturerId);
            
            CreateTable(
                "dbo.CourseMaterial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        FilePath = c.String(nullable: false),
                        FileType = c.String(maxLength: 50),
                        SubjectId = c.Int(nullable: false),
                        TopicId = c.Int(nullable: false),
                        UploadDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.SubjectId)
                .Index(t => t.TopicId);
            
            CreateTable(
                "dbo.Topic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        SubjectId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.VideoLesson",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        VideoUrl = c.String(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        TopicId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.SubjectId)
                .Index(t => t.TopicId);
            
            CreateTable(
                "dbo.Quiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        SubjectId = c.Int(nullable: false),
                        TopicId = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.SubjectId)
                .Index(t => t.TopicId);
            
            CreateTable(
                "dbo.QuizQuestion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionText = c.String(nullable: false),
                        QuizId = c.Int(nullable: false),
                        CorrectOptionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.QuizOption",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OptionText = c.String(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizQuestion", t => t.QuestionId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.OfflineSavedItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        ItemType = c.String(nullable: false, maxLength: 50),
                        ItemId = c.Int(nullable: false),
                        Title = c.String(maxLength: 200),
                        SavedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.StudentId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.PersonalNote",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        Content = c.String(),
                        TopicId = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        SubjectId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.StudentId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.StudentId)
                .Index(t => t.TopicId)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.ProgressRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        SubjectId = c.Int(),
                        TopicId = c.Int(),
                        ActivityType = c.String(maxLength: 50),
                        ActivityName = c.String(maxLength: 200),
                        Score = c.Int(),
                        CompletedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.StudentId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.StudentId)
                .Index(t => t.SubjectId)
                .Index(t => t.TopicId);
            
            CreateTable(
                "dbo.QuizAttempt",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        QuizId = c.Int(nullable: false),
                        Score = c.Int(nullable: false),
                        TotalQuestions = c.Int(nullable: false),
                        Percentage = c.Double(nullable: false),
                        AttemptDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId)
                .ForeignKey("dbo.User", t => t.StudentId)
                .Index(t => t.StudentId)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.StudentAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttemptId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        SelectedOptionId = c.Int(nullable: false),
                        IsCorrect = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizAttempt", t => t.AttemptId)
                .ForeignKey("dbo.QuizQuestion", t => t.QuestionId)
                .Index(t => t.AttemptId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.StudyPlan",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        Topics = c.String(nullable: false),
                        TestDate = c.DateTime(nullable: false),
                        DesiredMark = c.Int(nullable: false),
                        PlanContent = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.StudentId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .Index(t => t.StudentId)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.StudyRoom",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 150),
                        Description = c.String(maxLength: 500),
                        CreatedByStudentId = c.Int(nullable: false),
                        SubjectId = c.Int(),
                        TopicId = c.Int(),
                        MeetingUrl = c.String(),
                        IsLive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        EndedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatedByStudentId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.CreatedByStudentId)
                .Index(t => t.SubjectId)
                .Index(t => t.TopicId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudyRoom", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.StudyRoom", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.StudyRoom", "CreatedByStudentId", "dbo.User");
            DropForeignKey("dbo.StudyPlan", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.StudyPlan", "StudentId", "dbo.User");
            DropForeignKey("dbo.QuizAttempt", "StudentId", "dbo.User");
            DropForeignKey("dbo.QuizAttempt", "QuizId", "dbo.Quiz");
            DropForeignKey("dbo.StudentAnswer", "QuestionId", "dbo.QuizQuestion");
            DropForeignKey("dbo.StudentAnswer", "AttemptId", "dbo.QuizAttempt");
            DropForeignKey("dbo.ProgressRecord", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.ProgressRecord", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.ProgressRecord", "StudentId", "dbo.User");
            DropForeignKey("dbo.PersonalNote", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.PersonalNote", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.PersonalNote", "StudentId", "dbo.User");
            DropForeignKey("dbo.OfflineSavedItem", "StudentId", "dbo.User");
            DropForeignKey("dbo.ChatMessage", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.ChatMessage", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.Quiz", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.Quiz", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.QuizQuestion", "QuizId", "dbo.Quiz");
            DropForeignKey("dbo.QuizOption", "QuestionId", "dbo.QuizQuestion");
            DropForeignKey("dbo.Subject", "LecturerId", "dbo.User");
            DropForeignKey("dbo.CourseMaterial", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.VideoLesson", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.VideoLesson", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.Topic", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.CourseMaterial", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.ChatMessage", "StudentId", "dbo.User");
            DropForeignKey("dbo.AiSummary", "StudentId", "dbo.User");
            DropIndex("dbo.StudyRoom", new[] { "TopicId" });
            DropIndex("dbo.StudyRoom", new[] { "SubjectId" });
            DropIndex("dbo.StudyRoom", new[] { "CreatedByStudentId" });
            DropIndex("dbo.StudyPlan", new[] { "SubjectId" });
            DropIndex("dbo.StudyPlan", new[] { "StudentId" });
            DropIndex("dbo.StudentAnswer", new[] { "QuestionId" });
            DropIndex("dbo.StudentAnswer", new[] { "AttemptId" });
            DropIndex("dbo.QuizAttempt", new[] { "QuizId" });
            DropIndex("dbo.QuizAttempt", new[] { "StudentId" });
            DropIndex("dbo.ProgressRecord", new[] { "TopicId" });
            DropIndex("dbo.ProgressRecord", new[] { "SubjectId" });
            DropIndex("dbo.ProgressRecord", new[] { "StudentId" });
            DropIndex("dbo.PersonalNote", new[] { "SubjectId" });
            DropIndex("dbo.PersonalNote", new[] { "TopicId" });
            DropIndex("dbo.PersonalNote", new[] { "StudentId" });
            DropIndex("dbo.OfflineSavedItem", new[] { "StudentId" });
            DropIndex("dbo.QuizOption", new[] { "QuestionId" });
            DropIndex("dbo.QuizQuestion", new[] { "QuizId" });
            DropIndex("dbo.Quiz", new[] { "TopicId" });
            DropIndex("dbo.Quiz", new[] { "SubjectId" });
            DropIndex("dbo.VideoLesson", new[] { "TopicId" });
            DropIndex("dbo.VideoLesson", new[] { "SubjectId" });
            DropIndex("dbo.Topic", new[] { "SubjectId" });
            DropIndex("dbo.CourseMaterial", new[] { "TopicId" });
            DropIndex("dbo.CourseMaterial", new[] { "SubjectId" });
            DropIndex("dbo.Subject", new[] { "LecturerId" });
            DropIndex("dbo.ChatMessage", new[] { "TopicId" });
            DropIndex("dbo.ChatMessage", new[] { "SubjectId" });
            DropIndex("dbo.ChatMessage", new[] { "StudentId" });
            DropIndex("dbo.AiSummary", new[] { "StudentId" });
            DropTable("dbo.StudyRoom");
            DropTable("dbo.StudyPlan");
            DropTable("dbo.StudentAnswer");
            DropTable("dbo.QuizAttempt");
            DropTable("dbo.ProgressRecord");
            DropTable("dbo.PersonalNote");
            DropTable("dbo.OfflineSavedItem");
            DropTable("dbo.QuizOption");
            DropTable("dbo.QuizQuestion");
            DropTable("dbo.Quiz");
            DropTable("dbo.VideoLesson");
            DropTable("dbo.Topic");
            DropTable("dbo.CourseMaterial");
            DropTable("dbo.Subject");
            DropTable("dbo.ChatMessage");
            DropTable("dbo.User");
            DropTable("dbo.AiSummary");
        }
    }
}
