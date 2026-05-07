# SchoolGradesMvcSite.Tests

Набір xUnit-тестів для MVC-проєкту School Grades Management System.

## Що покрито
- не-BD сценарії: логін, реєстрація через Identity, вихід, зміна пароля;
- адміністрування користувачів через Identity;
- перевірка ролей і атрибутів авторизації;
- сценарії з InMemory DbContext: учні, предмети, оцінки, рейтинг, призначення предметів.

## Запуск

### Visual Studio
- Відкрити `SchoolGradesMvcSite-with-tests.sln`
- Відкрити **Test Explorer**
- Натиснути **Run All**

### CLI
```powershell
dotnet test .\SchoolGradesMvcSite-with-tests.sln
```

## Примітка
Частина тестів не використовує базу даних взагалі і перевіряє чисту бізнес-логіку контролерів через `Moq` та `Identity`-моки.
