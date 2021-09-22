Деобфускатор результатов выборов 2021
==============

На вход требуется сохранённая html страница с сайта izbirkom.ru.
В выходной файл запишется версия страницы без интерактивных элементов и стилей в удобном для дальнейшего анализа виде.

Обфускатор может работать в полностью оффлайн режиме, если у него есть доступ к шрифтам, которые имеются в количестве 100 штук на сайте избиркома. Они кешируются в папку `fonts_cache`. Если шрифт с необходимым именем уже есть, то загружаться снова он не будет.


Для запуска требуется [.net 5.0 SDK](https://dotnet.microsoft.com/download), должно работать на Windows/Linux/Mac.

![Пример результата](https://raw.githubusercontent.com/ulex/izbirkom21/master/samples/result.png)


[Исходная страница](https://raw.githubusercontent.com/ulex/izbirkom21/master/samples/test.in.html) была получена с помощью команды
```
curl -A 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:92.0) Gecko/20100101 Firefox/92.0' 'http://www.crimea.vybory.izbirkom.ru/region/izbirkom?action=show&root=1000050&tvd=100100225883448&vrn=100100225883172&prver=0&pronetvd=null&region=19&sub_region=19&type=463&report_mode=null' > test.in.html
```
[Пример деобфусцированного результата](https://raw.githubusercontent.com/ulex/izbirkom21/master/samples/test.out.html), который был получен с помощью команды
```
dotnet run --project izbirkom21 samples/test.in.html samples/test.out.html
```

Работа в пакетном режиме
----

Теперь можно запускать приложение на директориях. В этом случае, будут пытаться деобфусцироваться все файлы, которые есть в параметре 1 и записываться в директорию, указанную в параметре 2. Этот режим существенно быстрее обработки файлов по одному и на моём оборудовании обрабатывает примерно 40 файлов в секунду.
```
dotnet run --configuration Release --project izbirkom21 samples outDirectory
```

Сырые данные
----

Пользователь @illusionofchaos предлагает уже загруженные данные и шрифты с ЦИК по федеральному избирательному округу:  
https://github.com/illusionofchaos/IzbirkomRipper/releases/tag/0.0.1

Достоверность данных я не гарантирую


Используемые зависимости
----

https://github.com/LayoutFarm/Typography  
MIT

https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE  
MIT

https://github.com/TylerBrinks/ExCSS  
MIT

