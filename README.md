# Network Connectivity Tester

Tento projekt je konzolová aplikace, která umožňuje testování síťové konektivity pomocí TCP a UDP.

## Funkce
- **Režim vysílání**: Odesílání zpráv na zvolený port protokolem TCP nebo UDP.
- **Režim naslouchání**: Naslouchání příchozím zprávám na daných portech.
- **Podpora více portů**: Možnost zadání jednoho portu nebo intervalu (např. 5000-5010).
- **Podpora více protokolů**: TCP, UDP nebo oba současně.
- **Logování**: Používá knihovnu Serilog pro výstupy do konzole a logovacího souboru (`log.txt`).

## Požadavky
- .NET SDK
- Knihovna [Serilog](https://github.com/serilog/serilog)

## Instalace
1. Naklonujte si repozitář nebo stáhněte zdrojový kód.
2. Otevřete terminál v adresáři projektu.
3. Spusťte příkaz pro sestavení aplikace:
   ```sh
   dotnet build
   ```
4. Spusťte aplikaci:
   ```sh
   dotnet run
   ```

## Použití
Po spuštění se zobrazí interaktivní výběr režimu:
1. **Vysílání** (odesílání zpráv)
2. **Naslouchání** (příjem zpráv)

Poté vyberte protokol:
- `TCP` – Pouze TCP spojení
- `UDP` – Pouze UDP spojení
- `2` – Oba protokoly současně

Aplikace následně požádá o zadání portu nebo intervalu portů a v případě vysílání také o cílovou IP adresu.

## Logování
Logy jsou ukládány do konzole i do souboru `log.txt`. Informace jsou formátovány s úrovněmi:
- `INF` – Informace o běhu aplikace (např. odeslané a přijaté zprávy).
- `WRN` – Varování při chybách v komunikaci.
- `ERR` – Kritické chyby.

## Ukázka výstupu
```
2025-02-13 11:54:00.713 [INF] (TCP) Odeslána zpráva na 5003
2025-02-13 11:54:02.126 [INF] (UDP) Přijata zpráva na portu 5003: Test message
```
