# 0001 — Walidacja numeru rachunku NRB

Zlecający: człowiek (właściciel modułu `Accounts`). Wykonawca: agent.

## Cel

Formularz przelewu przyjmuje dziś dowolny napis jako numer rachunku odbiorcy. Błąd wychodzi dopiero
w systemie rozliczeniowym. Moduł `Accounts` ma umieć powiedzieć, czy napis jest poprawnym polskim
numerem rachunku (NRB — Numer Rachunku Bankowego, 26 cyfr), a jeśli nie — **dlaczego**.

## Kryteria akceptacji

- **K1** Odstępy i myślniki są ignorowane; prefiks kraju `PL` (dowolną wielkością liter) jest dopuszczalny.
- **K2** Po normalizacji numer ma dokładnie 26 cyfr. Inna długość → przyczyna „zła długość”; znak inny niż cyfra → przyczyna „niedozwolony znak”.
- **K3** Suma kontrolna liczona algorytmem IBAN (reszta z dzielenia przez 97 równa 1, kraj `PL` = `2521`). Niezgodna → przyczyna „zła suma kontrolna”.
- **K4** Wynik podaje PRZYCZYNĘ odrzucenia po imieniu (typ wyliczeniowy), nie samo „prawda/fałsz”.
- **K5** Dla poprawnego numeru wynik zawiera 8-cyfrowy numer rozliczeniowy banku (pozycje 3–10 numeru NRB).
- **K6** Wejście puste albo `null` → przyczyna „pusty”; prefiks kraju inny niż `PL` → przyczyna „nieobsługiwany kraj”.

## Poza zakresem

- Sprawdzanie, czy numer rozliczeniowy istnieje w wykazie banków.
- Numery zagraniczne (IBAN innych krajów).
- Zmiany w formularzu przelewu — to osobne zadanie.

## Materiał do testów

Wzorcowy polski numer z dokumentacji standardu IBAN: `PL61 1090 1014 0000 0712 1981 2874`.
