using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ThinkBridge.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
            //// This tells EF to use your initializer
            //Database.SetInitializer(new ApplicationDbInitializer());
        }
        public DbSet<StudyRoom> StudyRooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<VideoLesson> VideoLessons { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<ProgressRecord> ProgressRecords { get; set; }
        public DbSet<PersonalNote> PersonalNotes { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AiSummary> AiSummaries { get; set; }
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<OfflineSavedItem> OfflineSavedItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Disable cascade delete on all relationships to prevent multiple cascade paths
            modelBuilder.Entity<StudyRoom>()
                .HasRequired(sr => sr.CreatedByStudent)
                .WithMany()
                .HasForeignKey(sr => sr.CreatedByStudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StudyRoom>()
                .HasOptional(sr => sr.Subject)
                .WithMany()
                .HasForeignKey(sr => sr.SubjectId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StudyRoom>()
                .HasOptional(sr => sr.Topic)
                .WithMany()
                .HasForeignKey(sr => sr.TopicId)
                .WillCascadeOnDelete(false);
            // CourseMaterial -> Subject
            modelBuilder.Entity<PersonalNote>()
                .HasOptional(pn => pn.Subject)
                .WithMany()
                .HasForeignKey(pn => pn.SubjectId)
                .WillCascadeOnDelete(false);

            // CourseMaterial -> Subject
            modelBuilder.Entity<CourseMaterial>()
                .HasRequired(cm => cm.Subject)
                .WithMany(s => s.CourseMaterials)
                .HasForeignKey(cm => cm.SubjectId)
                .WillCascadeOnDelete(false);

            // CourseMaterial -> Topic
            modelBuilder.Entity<CourseMaterial>()
                .HasRequired(cm => cm.Topic)
                .WithMany(t => t.CourseMaterials)
                .HasForeignKey(cm => cm.TopicId)
                .WillCascadeOnDelete(false);

            // VideoLesson -> Subject
            modelBuilder.Entity<VideoLesson>()
                .HasRequired(vl => vl.Subject)
                .WithMany(s => s.VideoLessons)
                .HasForeignKey(vl => vl.SubjectId)
                .WillCascadeOnDelete(false);

            // VideoLesson -> Topic
            modelBuilder.Entity<VideoLesson>()
                .HasRequired(vl => vl.Topic)
                .WithMany(t => t.VideoLessons)
                .HasForeignKey(vl => vl.TopicId)
                .WillCascadeOnDelete(false);

            // Quiz -> Subject
            modelBuilder.Entity<Quiz>()
                .HasRequired(q => q.Subject)
                .WithMany(s => s.Quizzes)
                .HasForeignKey(q => q.SubjectId)
                .WillCascadeOnDelete(false);

            // Quiz -> Topic
            modelBuilder.Entity<Quiz>()
                .HasOptional(q => q.Topic)
                .WithMany()
                .HasForeignKey(q => q.TopicId)
                .WillCascadeOnDelete(false);

            // QuizQuestion -> Quiz
            modelBuilder.Entity<QuizQuestion>()
                .HasRequired(qq => qq.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qq => qq.QuizId)
                .WillCascadeOnDelete(false);

            // QuizOption -> QuizQuestion
            modelBuilder.Entity<QuizOption>()
                .HasRequired(qo => qo.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(qo => qo.QuestionId)
                .WillCascadeOnDelete(false);

            // QuizAttempt -> Student (User)
            modelBuilder.Entity<QuizAttempt>()
                .HasRequired(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentId)
                .WillCascadeOnDelete(false);

            // QuizAttempt -> Quiz
            modelBuilder.Entity<QuizAttempt>()
                .HasRequired(qa => qa.Quiz)
                .WithMany()
                .HasForeignKey(qa => qa.QuizId)
                .WillCascadeOnDelete(false);

            // StudentAnswer -> Attempt
            modelBuilder.Entity<StudentAnswer>()
                .HasRequired(sa => sa.Attempt)
                .WithMany(a => a.Answers)
                .HasForeignKey(sa => sa.AttemptId)
                .WillCascadeOnDelete(false);

            // StudentAnswer -> Question
            modelBuilder.Entity<StudentAnswer>()
                .HasRequired(sa => sa.Question)
                .WithMany()
                .HasForeignKey(sa => sa.QuestionId)
                .WillCascadeOnDelete(false);

            // ProgressRecord -> Student
            modelBuilder.Entity<ProgressRecord>()
                .HasRequired(pr => pr.Student)
                .WithMany()
                .HasForeignKey(pr => pr.StudentId)
                .WillCascadeOnDelete(false);

            // ProgressRecord -> Subject
            modelBuilder.Entity<ProgressRecord>()
                .HasOptional(pr => pr.Subject)
                .WithMany()
                .HasForeignKey(pr => pr.SubjectId)
                .WillCascadeOnDelete(false);

            // ProgressRecord -> Topic
            modelBuilder.Entity<ProgressRecord>()
                .HasOptional(pr => pr.Topic)
                .WithMany()
                .HasForeignKey(pr => pr.TopicId)
                .WillCascadeOnDelete(false);

            // PersonalNote -> Student
            modelBuilder.Entity<PersonalNote>()
                .HasRequired(pn => pn.Student)
                .WithMany()
                .HasForeignKey(pn => pn.StudentId)
                .WillCascadeOnDelete(false);

            // PersonalNote -> Topic
            modelBuilder.Entity<PersonalNote>()
                .HasOptional(pn => pn.Topic)
                .WithMany()
                .HasForeignKey(pn => pn.TopicId)
                .WillCascadeOnDelete(false);

            // ChatMessage -> Student
            modelBuilder.Entity<ChatMessage>()
                .HasRequired(cm => cm.Student)
                .WithMany()
                .HasForeignKey(cm => cm.StudentId)
                .WillCascadeOnDelete(false);

            // ChatMessage -> Subject
            modelBuilder.Entity<ChatMessage>()
                .HasOptional(cm => cm.Subject)
                .WithMany()
                .HasForeignKey(cm => cm.SubjectId)
                .WillCascadeOnDelete(false);

            // ChatMessage -> Topic
            modelBuilder.Entity<ChatMessage>()
                .HasOptional(cm => cm.Topic)
                .WithMany()
                .HasForeignKey(cm => cm.TopicId)
                .WillCascadeOnDelete(false);

            // AiSummary -> Student
            modelBuilder.Entity<AiSummary>()
                .HasRequired(ais => ais.Student)
                .WithMany()
                .HasForeignKey(ais => ais.StudentId)
                .WillCascadeOnDelete(false);

            // StudyPlan -> Student
            modelBuilder.Entity<StudyPlan>()
                .HasRequired(sp => sp.Student)
                .WithMany()
                .HasForeignKey(sp => sp.StudentId)
                .WillCascadeOnDelete(false);

            // StudyPlan -> Subject
            modelBuilder.Entity<StudyPlan>()
                .HasRequired(sp => sp.Subject)
                .WithMany()
                .HasForeignKey(sp => sp.SubjectId)
                .WillCascadeOnDelete(false);

            // OfflineSavedItem -> Student
            modelBuilder.Entity<OfflineSavedItem>()
                .HasRequired(osi => osi.Student)
                .WithMany()
                .HasForeignKey(osi => osi.StudentId)
                .WillCascadeOnDelete(false);

            // Topic -> Subject
            modelBuilder.Entity<Topic>()
                .HasRequired(t => t.Subject)
                .WithMany(s => s.Topics)
                .HasForeignKey(t => t.SubjectId)
                .WillCascadeOnDelete(false);

            // Subject -> Lecturer (User)
            modelBuilder.Entity<Subject>()
                .HasRequired(s => s.Lecturer)
                .WithMany()
                .HasForeignKey(s => s.LecturerId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        // ========== SEED DATA GOES HERE ==========
        public class ApplicationDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
        {
            protected override void Seed(ApplicationDbContext context)
            {
                // 1. USERS
                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@thinkbridge.edu",
                    FullName = "System Administrator",
                    Role = "Admin",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var lecturer = new User
                {
                    Username = "lecturer",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("lecturer123"),
                    Email = "lecturer@thinkbridge.edu",
                    FullName = "Dr. John Smith",
                    Role = "Lecturer",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var student = new User
                {
                    Username = "student",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    Email = "student@thinkbridge.edu",
                    FullName = "Jane Doe",
                    Role = "Student",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.Users.Add(admin);
                context.Users.Add(lecturer);
                context.Users.Add(student);
                context.SaveChanges();

                // 2. SUBJECTS
                var mathSubject = new Subject
                {
                    Name = "Advanced Mathematics",
                    Description = "Calculus, algebra, and mathematical analysis",
                    LecturerId = lecturer.Id,
                    CreatedAt = DateTime.Now
                };

                var csSubject = new Subject
                {
                    Name = "Computer Science Fundamentals",
                    Description = "Programming, algorithms, and data structures",
                    LecturerId = lecturer.Id,
                    CreatedAt = DateTime.Now
                };

                context.Subjects.Add(mathSubject);
                context.Subjects.Add(csSubject);
                context.SaveChanges();

                // 3. TOPICS
                var topics = new List<Topic>
            {
                new Topic { Name = "Linear Algebra", Description = "Vectors, matrices, and linear transformations", SubjectId = mathSubject.Id },
                new Topic { Name = "Calculus I", Description = "Limits, derivatives, and integrals", SubjectId = mathSubject.Id },
                new Topic { Name = "Differential Equations", Description = "ODEs and PDEs", SubjectId = mathSubject.Id },
                new Topic { Name = "Data Structures", Description = "Arrays, lists, trees, and graphs", SubjectId = csSubject.Id },
                new Topic { Name = "Algorithms", Description = "Sorting, searching, and complexity analysis", SubjectId = csSubject.Id }
            };

                context.Topics.AddRange(topics);
                context.SaveChanges();

                // 4. COURSE MATERIALS
                var materials = new List<CourseMaterial>
            {
                new CourseMaterial
                {
                    Title = "Linear Algebra Notes",
                    Description = "Complete notes on vectors and matrices",
                    FilePath = "/Uploads/CourseMaterials/linear_algebra.pdf",
                    FileType = "PDF",
                    SubjectId = mathSubject.Id,
                    TopicId = topics[0].Id
                },
                new CourseMaterial
                {
                    Title = "Calculus Formula Sheet",
                    Description = "Essential formulas for Calculus I",
                    FilePath = "/Uploads/CourseMaterials/calculus_formulas.docx",
                    FileType = "DOCX",
                    SubjectId = mathSubject.Id,
                    TopicId = topics[1].Id
                },
                new CourseMaterial
                {
                    Title = "Data Structures Guide",
                    Description = "Comprehensive guide to data structures",
                    FilePath = "/Uploads/CourseMaterials/ds_guide.pdf",
                    FileType = "PDF",
                    SubjectId = csSubject.Id,
                    TopicId = topics[3].Id
                }
            };

                context.CourseMaterials.AddRange(materials);
                context.SaveChanges();

                // 5. VIDEO LESSONS
                var videos = new List<VideoLesson>
            {
                new VideoLesson
                {
                    Title = "Introduction to Matrices",
                    Description = "Basic concepts of matrix operations and transformations",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    SubjectId = mathSubject.Id,
                    TopicId = topics[0].Id
                },
                new VideoLesson
                {
                    Title = "Derivatives Explained",
                    Description = "Understanding derivatives with practical examples",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    SubjectId = mathSubject.Id,
                    TopicId = topics[1].Id
                },
                new VideoLesson
                {
                    Title = "Binary Trees Tutorial",
                    Description = "Implementing binary trees in code with examples",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    SubjectId = csSubject.Id,
                    TopicId = topics[3].Id
                }
            };

                context.VideoLessons.AddRange(videos);
                context.SaveChanges();

                // 6. QUIZZES
                var quiz1 = new Quiz
                {
                    Title = "Linear Algebra Basics",
                    Description = "Test your understanding of vectors and matrices",
                    SubjectId = mathSubject.Id,
                    TopicId = topics[0].Id,
                    CreatedAt = DateTime.Now
                };

                var quiz2 = new Quiz
                {
                    Title = "Data Structures Quiz",
                    Description = "Arrays, lists, and trees",
                    SubjectId = csSubject.Id,
                    TopicId = topics[3].Id,
                    CreatedAt = DateTime.Now
                };

                context.Quizzes.Add(quiz1);
                context.Quizzes.Add(quiz2);
                context.SaveChanges();

                // 7. QUIZ QUESTIONS & OPTIONS
                var q1 = new QuizQuestion { QuestionText = "What is the determinant of a 2x2 matrix [[1,2],[3,4]]?", QuizId = quiz1.Id };
                var q2 = new QuizQuestion { QuestionText = "Which operation is NOT valid for matrices?", QuizId = quiz1.Id };
                context.QuizQuestions.Add(q1);
                context.QuizQuestions.Add(q2);
                context.SaveChanges();

                var opts1 = new List<QuizOption>
            {
                new QuizOption { OptionText = "-2", QuestionId = q1.Id },
                new QuizOption { OptionText = "2", QuestionId = q1.Id },
                new QuizOption { OptionText = "10", QuestionId = q1.Id },
                new QuizOption { OptionText = "-10", QuestionId = q1.Id }
            };
                context.QuizOptions.AddRange(opts1);
                context.SaveChanges();
                q1.CorrectOptionId = opts1[0].Id;

                var opts2 = new List<QuizOption>
            {
                new QuizOption { OptionText = "Addition", QuestionId = q2.Id },
                new QuizOption { OptionText = "Division", QuestionId = q2.Id },
                new QuizOption { OptionText = "Multiplication", QuestionId = q2.Id },
                new QuizOption { OptionText = "Transpose", QuestionId = q2.Id }
            };
                context.QuizOptions.AddRange(opts2);
                context.SaveChanges();
                q2.CorrectOptionId = opts2[1].Id;

                var q3 = new QuizQuestion { QuestionText = "What is the time complexity of binary search?", QuizId = quiz2.Id };
                var q4 = new QuizQuestion { QuestionText = "Which data structure uses LIFO?", QuizId = quiz2.Id };
                context.QuizQuestions.Add(q3);
                context.QuizQuestions.Add(q4);
                context.SaveChanges();

                var opts3 = new List<QuizOption>
            {
                new QuizOption { OptionText = "O(n)", QuestionId = q3.Id },
                new QuizOption { OptionText = "O(log n)", QuestionId = q3.Id },
                new QuizOption { OptionText = "O(n^2)", QuestionId = q3.Id },
                new QuizOption { OptionText = "O(1)", QuestionId = q3.Id }
            };
                context.QuizOptions.AddRange(opts3);
                context.SaveChanges();
                q3.CorrectOptionId = opts3[1].Id;

                var opts4 = new List<QuizOption>
            {
                new QuizOption { OptionText = "Queue", QuestionId = q4.Id },
                new QuizOption { OptionText = "Stack", QuestionId = q4.Id },
                new QuizOption { OptionText = "Array", QuestionId = q4.Id },
                new QuizOption { OptionText = "Linked List", QuestionId = q4.Id }
            };
                context.QuizOptions.AddRange(opts4);
                context.SaveChanges();
                q4.CorrectOptionId = opts4[1].Id;

                // 8. QUIZ ATTEMPTS
                var attempt1 = new QuizAttempt
                {
                    StudentId = student.Id,
                    QuizId = quiz1.Id,
                    Score = 1,
                    TotalQuestions = 2,
                    Percentage = 50,
                    AttemptDate = DateTime.Now.AddDays(-2)
                };
                context.QuizAttempts.Add(attempt1);
                context.SaveChanges();

                var attempt2 = new QuizAttempt
                {
                    StudentId = student.Id,
                    QuizId = quiz2.Id,
                    Score = 2,
                    TotalQuestions = 2,
                    Percentage = 100,
                    AttemptDate = DateTime.Now.AddDays(-1)
                };
                context.QuizAttempts.Add(attempt2);
                context.SaveChanges();

                // 9. STUDENT ANSWERS
                context.StudentAnswers.Add(new StudentAnswer { AttemptId = attempt1.Id, QuestionId = q1.Id, SelectedOptionId = opts1[0].Id, IsCorrect = true });
                context.StudentAnswers.Add(new StudentAnswer { AttemptId = attempt1.Id, QuestionId = q2.Id, SelectedOptionId = opts2[0].Id, IsCorrect = false });
                context.StudentAnswers.Add(new StudentAnswer { AttemptId = attempt2.Id, QuestionId = q3.Id, SelectedOptionId = opts3[1].Id, IsCorrect = true });
                context.StudentAnswers.Add(new StudentAnswer { AttemptId = attempt2.Id, QuestionId = q4.Id, SelectedOptionId = opts4[1].Id, IsCorrect = true });
                context.SaveChanges();

                // 10. PROGRESS RECORDS
                var progressRecords = new List<ProgressRecord>
            {
                new ProgressRecord
                {
                    StudentId = student.Id,
                    SubjectId = mathSubject.Id,
                    TopicId = topics[0].Id,
                    ActivityType = "Quiz",
                    ActivityName = "Linear Algebra Basics",
                    Score = 50,
                    CompletedAt = DateTime.Now.AddDays(-2)
                },
                new ProgressRecord
                {
                    StudentId = student.Id,
                    SubjectId = csSubject.Id,
                    TopicId = topics[3].Id,
                    ActivityType = "Quiz",
                    ActivityName = "Data Structures Quiz",
                    Score = 100,
                    CompletedAt = DateTime.Now.AddDays(-1)
                },
                new ProgressRecord
                {
                    StudentId = student.Id,
                    SubjectId = mathSubject.Id,
                    TopicId = topics[0].Id,
                    ActivityType = "Lesson",
                    ActivityName = "Introduction to Matrices",
                    CompletedAt = DateTime.Now.AddDays(-3)
                },
                new ProgressRecord
                {
                    StudentId = student.Id,
                    SubjectId = csSubject.Id,
                    TopicId = topics[3].Id,
                    ActivityType = "Lesson",
                    ActivityName = "Binary Trees Tutorial",
                    CompletedAt = DateTime.Now.AddDays(-1)
                }
            };

                context.ProgressRecords.AddRange(progressRecords);
                context.SaveChanges();

                // 11. PERSONAL NOTES
                context.PersonalNotes.Add(new PersonalNote
                {
                    StudentId = student.Id,
                    Title = "Matrix Multiplication Tips",
                    Content = "Remember that matrix multiplication is not commutative. The order matters. Also, the number of columns in the first matrix must equal the number of rows in the second matrix. Determinants help find inverse matrices. Eigenvalues are important for diagonalization.",
                    TopicId = topics[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-5)
                });
                context.SaveChanges();

                base.Seed(context);
            }
        }
    }
}