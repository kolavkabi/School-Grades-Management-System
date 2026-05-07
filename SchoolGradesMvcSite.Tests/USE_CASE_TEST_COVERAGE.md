# Use case coverage

## Тести, не прив'язані до бази даних

### AccountControllerTests
1. **Login_Get_ReturnsViewAndPreservesReturnUrl**  
   Відображення сторінки входу з returnUrl.
2. **Login_Post_InvalidModel_ReturnsSameView**  
   Некоректна форма входу не виконує авторизацію.
3. **Login_Post_InactiveUser_ReturnsViewWithError**  
   Неактивний користувач не може увійти.
4. **Login_Post_ValidCredentials_RedirectsHome**  
   Активний користувач успішно входить у систему.
5. **Login_Post_WithLocalReturnUrl_RedirectsToThatUrl**  
   Після входу виконується повернення на локальну сторінку.
6. **Register_Get_ReturnsEmptyViewModel**  
   Відображення сторінки реєстрації.
7. **Register_Post_InvalidModel_ReturnsSameView**  
   Некоректна форма реєстрації повертає користувача на форму.
8. **Register_Post_IdentityCreationFailure_ReturnsViewWithErrors**  
   Помилка Identity під час створення акаунта показується у формі.
9. **Register_Post_ValidModel_CreatesUserAssignsRoleAndSignsIn**  
   Реєстрація через Identity створює користувача, видає роль User і виконує вхід.
10. **Logout_Post_SignsOutAndRedirectsHome**  
    Вихід із системи.
11. **ChangePassword_InvalidModel_ReturnsView**  
    Некоректна форма зміни пароля не відправляється далі.
12. **ChangePassword_UserMissing_RedirectsToLogin**  
    Якщо користувача не знайдено, виконується повернення на сторінку входу.
13. **ChangePassword_Failure_ReturnsViewWithErrors**  
    Помилки зміни пароля повертаються у ModelState.
14. **ChangePassword_Success_RedirectsHome**  
    Успішна зміна пароля.

### AdminControllerTests
15. **CreateUser_ValidModel_CreatesUserAndAssignsRole**  
    Адміністратор створює користувача і призначає роль.
16. **CreateUser_IdentityError_ReturnsViewWithErrors**  
    Помилки створення користувача від Identity показуються у формі.
17. **ToggleUser_FlipsUserActivity**  
    Адміністратор активує або деактивує користувача.
18. **DeleteUser_MainAdmin_IsProtected**  
    Головного адміністратора не можна видалити.

### AuthorizationAttributesTests
19. **Subjects_Index_AllowsAnonymousGuestAccess**  
    Гість може переглядати предмети.
20. **Account_Register_AllowsAnonymousAccess**  
    Реєстрація доступна без входу.
21. **Teachers_Controller_RequiresAdministratorRole**  
    Керування вчителями доступне лише адміністратору.
22. **Reports_Controller_RequiresStaffRoles**  
    Звіти доступні ролям User або Administrator.
23. **Students_ToggleStatus_RequiresAdministratorRole**  
    Зміна активності учня доступна лише адміністратору.

## Тести, що використовують InMemory DbContext
24. **Create_ValidStudent_PersistsAndRedirects**  
    Додавання учня.
25. **Import_ValidCsv_ImportsRows**  
    Імпорт учнів із CSV.
26. **Index_ReturnsOrderedSubjects_ForGuestUseCase**  
    Перегляд відсортованого списку предметів.
27. **Create_ValidGrade_PersistsAndRedirects**  
    Виставлення оцінки.
28. **StudentAverage_ReturnsCalculatedAverage**  
    Обчислення середнього бала учня.
29. **Ranking_FiltersByClassAndOrdersByAverageDescending**  
    Рейтинг учнів у межах класу.
30. **AssignSubject_Post_CreatesTeacherSubjectLinkOnlyOnce**  
    Призначення предмета вчителю без дублювання зв'язку.
