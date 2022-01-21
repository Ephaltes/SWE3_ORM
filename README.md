# SWE3_ORM

- Tabellen müssen vorher in der Datenbank erstellt worden sein und sollen so wie die Klasse heißen , sonst TableAttribute verwenden mit den Namen der Tabelle
- Primary Keys müssen integer sein, wenn der Primary Key auto increment ist, kann diese angegeben werden im PrimaryKey Attribut
- Im docker Ordner befindet sich eine Datenbank mit vordefinierten Tabellen für das TestProgramm
  - Docker Desktop starten
  - CMD im Ordner öffnen; "docker-compose up" eingeben zum starten; STRG+C und danach "docker-compose down" zum beenden und löschen des containers;
  - Sollte beim starten der Fehler kommen, das der Datenbank user nicht vorhanden ist oder dass das Passwort falsch ist
  muss das docker-compose down gemacht werden und dann nochmal docker-compose up dann sollte die datenbank funktionieren.
- Fluentapi kann über FluentApi.Get<T>() aufgerufen werden, am Ende das Execute aufrufen wobei T die Entität ist die man von der Datenbank holen will.
- Logging wird in die Debug Console rausgeschrieben, dafür muss man das programm als Debug starten, damit man die Debug Console sieht;
  Es wird nicht in die Standard Console rausgeschrieben, da es sonst zu unleserlich wäre