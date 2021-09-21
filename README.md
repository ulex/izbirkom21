Деобфускатор результатов выборов 2021
==============

На вход требуется сохранённая html страница с сайта izbirkom.ru.
В выходной файл запишется версия страницы без интерактивных элементов и стилей в удобном для дальнейшего анализа виде.

К сожалению, обфускатор может работать в полностью оффлайн режиме, только если у него есть доступ к шрифтам, которые (предположительно) имеются в достаточно большом, но счетном количестве на сайте избиркома. Они кешируются в папку `fonts_cache`. Если шрифт с необходимым именем уже есть, то загружаться снова он не будет.

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



Используемые зависимости:

https://github.com/LayoutFarm/Typography  
MIT

https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE  
MIT

https://github.com/TylerBrinks/ExCSS  
MIT

