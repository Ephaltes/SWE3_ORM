CREATE TABLE teachers(
id serial PRIMARY KEY,
name TEXT,
firstname TEXT,
gender INT,
birthdate DATE,
hiredate DATE,
salary INT
);


CREATE TABLE classes(
id serial PRIMARY KEY,
name TEXT,
teachersid INT,
CONSTRAINT fk_classes_teachears FOREIGN KEY (teachersid) REFERENCES teachers(id)
);

CREATE TABLE students(
id serial PRIMARY KEY,
name TEXT,
firstname TEXT,
gender INT,
birthdate DATE,
classesid INT,
grade INT,
CONSTRAINT fk_students_classes FOREIGN KEY (classesid) REFERENCES classes(id)
);

CREATE TABLE courses(
id serial PRIMARY KEY,
name TEXT,
active BOOLEAN,
teachersid INT,
CONSTRAINT fk_courses_teachers FOREIGN KEY (teachersid) REFERENCES teachers(id)
);

CREATE TABLE r_students_courses(
id serial PRIMARY KEY,
studentsid INT,
coursesid INT,
CONSTRAINT fk_r_students_courses_students FOREIGN KEY (studentsid) REFERENCES students(id),
CONSTRAINT fk_r_students_courses_courses FOREIGN KEY (coursesid) REFERENCES courses(id)
);