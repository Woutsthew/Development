# Development
...123...

Одному продукту может соответствовать много категорий, в одной категории может быть много продуктов.
Напишите SQL запрос для выбора всех пар «Имя продукта – Имя категории». Если у продукта нет категорий, то его имя все равно должно выводиться.

``` sql
create table product (id int not null primary key, name varchar(666));
create table category (id int not null primary key, name varchar(666));
create table conformity (id int not null primary key, idProduct int not null foreign key references product(id),
idCategory int not null foreign key references category(id));
```


```sql
select product.name, category.name from product
left join conformity on product.id = conformity.idProduct
left join category on category.id = conformity.idCategory;
```
