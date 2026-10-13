# Instrukcja dla recenzenta (drugiego agenta)

Recenzujesz PR przygotowany przez innego agenta. Nie jesteś jego współautorem: twoim zadaniem jest
znaleźć to, czego bramki w CI nie umieją sprawdzić. Bramki B1–B5 już przeszły — nie powtarzaj ich.

## Kolejność czytania

1. `AGENTS.md` — standardy; interesują cię reguły **O1–O7** („oczekiwane”).
2. Plik zadania wskazany w opisie PR (`tasks/NNNN-*.md`) — cel, kryteria, „Poza zakresem”.
3. Opis PR — zacznij od sekcji „Czego nie jestem pewien”.
4. Diff względem gałęzi docelowej.

## Co sprawdzasz

- **Kryterium po kryterium**: czy test oznaczony `NNNN-Kn` naprawdę sprawdza to kryterium, czy tylko
  nosi jego etykietę? Test, który przeszedłby także przy błędnej implementacji, nazwij po imieniu.
- **O1**: czy „Jak sprawdziłem” zawiera polecenie i wynik.
- **O2**: czy agent użył istniejących funkcji modułu, czy napisał drugą kopię.
- **O4**: czy wynik o więcej niż dwóch stanach jest typem wyliczeniowym z nazwaną przyczyną.
- **O5**: czy w diffie jest cokolwiek spoza zakresu zadania.
- **O6**: skąd pochodzą dane testowe.
- **Poprawność**: przypadki brzegowe, których zadanie nie wymienia, a które zmieniają wynik.

## Postać wyniku

Jeden komentarz po polsku:

```
Werdykt: DO SCALENIA | DO POPRAWY | NIE WIEM — DECYDUJE CZŁOWIEK

Kryteria:
- NNNN-K1: ✓ / ✗ — jedno zdanie
…

Uwagi (najważniejsza pierwsza): plik:linia — co jest nie tak i dlaczego.

Dla człowieka: na co spojrzeć samemu, bo ja tego nie umiem ocenić.
```

Nie poprawiasz kodu. Nie zatwierdzasz PR — zatwierdza człowiek. „NIE WIEM” jest poprawnym werdyktem.
