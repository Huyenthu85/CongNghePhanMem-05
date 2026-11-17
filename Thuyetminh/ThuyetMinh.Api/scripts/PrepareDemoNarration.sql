-- Development demo only.
USE [ThuyetMinhDB_Test];
GO
SET XACT_ABORT ON;
DECLARE @Images TABLE (ShopId bigint, ShopName nvarchar(200), Photo nvarchar(2048));
INSERT INTO @Images VALUES
 (1, N'Quán Ăn Gia Đình Phố', N'https://images.unsplash.com/photo-1552566626-52f8b828add9?auto=format&fit=crop&w=1100&q=80'),
 (2, N'Bún Chả Hà Nội 36 Phố', N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=1100&q=80'),
 (3, N'Phở Bò Sài Gòn Xưa', N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=1100&q=80'),
 (4, N'Ốc Đêm Sài Thành', N'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1100&q=80'),
 (5, N'Cơm Tấm Ba Ghiền', N'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1100&q=80');

BEGIN TRANSACTION;
UPDATE sub
SET ImageUrlsRaw = img.Photo
FROM dbo.ShopSubmissions sub
JOIN dbo.Shops s ON s.Id = sub.ShopId AND s.PublishedVersionId = sub.Id
JOIN @Images img ON img.ShopId = s.Id AND img.ShopName = s.Name
WHERE s.PublishStatus = 2 AND sub.Status = 2
  AND NULLIF(LTRIM(RTRIM(sub.ImageUrlsRaw)), '') IS NULL;

UPDATE job
SET Status = 'Pending', AttemptCount = 0, LastAttemptAt = NULL,
    CompletedAt = NULL, ErrorMessage = NULL
FROM dbo.ProcessingJobs job
JOIN dbo.ShopSubmissions sub ON sub.Id = job.ShopSubmissionId
JOIN dbo.Shops s ON s.Id = sub.ShopId AND s.PublishedVersionId = sub.Id
JOIN @Images img ON img.ShopId = s.Id AND img.ShopName = s.Name
JOIN dbo.Languages lang ON lang.Id = job.LanguageId
WHERE s.PublishStatus = 2 AND sub.Status = 2 AND job.Status = 'Failed'
  AND lang.IsActive = 1 AND lang.Code IN ('vi','en','zh','km')
  AND NULLIF(LTRIM(RTRIM(sub.Description)), '') IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM dbo.Translations t JOIN dbo.Audios a ON a.TranslationId = t.Id
    WHERE t.ShopSubmissionId = sub.Id AND t.LanguageId = lang.Id
      AND t.Status = 'Success' AND a.Status = 'Success' AND a.DurationSeconds > 0
  );
COMMIT;

SELECT s.Id, s.Name, s.PublishedVersionId, l.Code, j.Status AS JobStatus,
    j.AttemptCount, j.ErrorMessage, a.Status AS AudioStatus, a.FilePath
FROM dbo.Shops s
JOIN @Images img ON img.ShopId = s.Id AND img.ShopName = s.Name
CROSS JOIN dbo.Languages l
LEFT JOIN dbo.Translations t ON t.ShopSubmissionId = s.PublishedVersionId AND t.LanguageId = l.Id
LEFT JOIN dbo.Audios a ON a.TranslationId = t.Id
OUTER APPLY (
    SELECT TOP (1) p.Status, p.AttemptCount, p.ErrorMessage
    FROM dbo.ProcessingJobs p
    WHERE p.ShopSubmissionId = s.PublishedVersionId AND p.LanguageId = l.Id
    ORDER BY p.Id DESC
) j
WHERE l.Code IN ('vi','en','zh','km')
ORDER BY s.Id, l.Code;
