# Відповідність SRS і командних змін

## Версія
Поточна навчальна версія документації: **SRS 1.1**.

## Реалізовані ролі
- **Guest**: Home, About, Subjects (read-only), Login.
- **User**: Students, Subjects CRUD, Grades CRUD, Reports, ChangePassword, Logout.
- **Administrator**: усе з ролі User + Teachers CRUD, AssignSubject, Admin Dashboard, Users management, role control.

## Додаткові командні use case
1. **Export Student Report to CSV**
   - Реалізація: `ReportsController.ExportStudentReportCsv(int studentId)`
   - Інтерфейс: кнопка на сторінці `Reports/StudentReport`

2. **Filter Students by Class**
   - Реалізація: `StudentsController.Index(string? search, string? className)`
   - Інтерфейс: форма пошуку і dropdown на сторінці `Students/Index`

3. **View Top Student in Class**
   - Реалізація: `ReportsController.TopStudentInClass(string className)`
   - Інтерфейс: сторінка `Reports/TopStudentInClass`

4. **Add Teacher Comment to Grade**
   - Реалізація: `GradesController.EditComment(int id)`
   - Інтерфейс: сторінка `Grades/EditComment`

## Вплив на class diagram
- Моделі домену **не змінювалися**, бо всі 4 функції використовують уже наявні сутності `Student`, `Teacher`, `Subject`, `Grade`.
- Додано лише допоміжні `ViewModel` для UI-рівня.
