-- Erweitere StudentGrades Tabelle um Grade4 und Grade5
ALTER TABLE StudentGrades
ADD Grade4 DECIMAL(3,1) NULL,
    Grade5 DECIMAL(3,1) NULL;
