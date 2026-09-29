-- Füge GradeCount-Spalte zur Modules-Tabelle hinzu
ALTER TABLE Modules
ADD GradeCount INT DEFAULT 3;

-- Setze alle bestehenden Module auf 3 Noten pro Standard
UPDATE Modules SET GradeCount = 3 WHERE GradeCount IS NULL;
