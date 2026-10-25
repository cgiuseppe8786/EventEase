# EventEase

**EventEase** è un'applicazione **Blazor WebAssembly (.NET 10)** per la gestione di eventi aziendali e sociali:
gli utenti scoprono gli eventi, si registrano tramite un modulo con convalida, ritrovano le proprie iscrizioni
grazie alla sessione utente e gli organizzatori registrano le presenze all'ingresso.

Il progetto riunisce le tre attività del corso *Blazor for Front-End Development*:

1. **Attività 1**: generazione del codice Blazor con Microsoft Copilot (componente Event Card, binding bidirezionale, routing).
2. **Attività 2**: debug e ottimizzazione (convalida degli input, errori di routing, prestazioni).
3. **Attività 3**: funzionalità avanzate (modulo di registrazione, stato della sessione utente, monitoraggio delle presenze).

---

## Requisiti del progetto e dove trovarli

| Requisito | Implementazione |
|---|---|
| Repository GitHub | Questo repository, con CI su GitHub Actions ([ci.yml](.github/workflows/ci.yml)) che compila ed esegue i test a ogni push, e deploy automatico su GitHub Pages ([deploy.yml](.github/workflows/deploy.yml)). |
| Componente **Event Card** con campi e **binding bidirezionale** | [EventCard.razor](src/EventEase/Components/EventCard.razor): mostra nome, data, luogo, categoria e capienza. Espone `Event` + `EventChanged`, quindi il genitore usa `@bind-Event` (vedi [EventDetails.razor](src/EventEase/Pages/EventDetails.razor)). In modifica, gli input (`@bind-Value`) sono legati a una copia ("draft"), così "Annulla" non altera l'originale. |
| **Routing** implementato e debuggato | Rotte con vincoli di tipo (`/events/{Id:int}`), parametri opzionali (`/attendance/{EventId:int}`), query string (`/events?category=Workshop`), pagina 404 (`NotFoundPage` del `Router`) e messaggio dedicato per gli id inesistenti ([EventNotFound.razor](src/EventEase/Components/EventNotFound.razor)). |
| **Ottimizzazione** (convalida input, errori di routing, prestazioni) | Vedi la sezione [Debug e ottimizzazioni](#debug-e-ottimizzazioni). |
| **Funzionalità avanzate** | Modulo di registrazione con convalida ([Register.razor](src/EventEase/Pages/Register.razor)), sessione utente ([UserSessionService.cs](src/EventEase/Services/UserSessionService.cs)), monitoraggio presenze con check-in e statistiche ([Attendance.razor](src/EventEase/Pages/Attendance.razor), [AttendanceService.cs](src/EventEase/Services/AttendanceService.cs)). |
| Sintesi dell'assistenza di Copilot | Sezione [Come Copilot mi ha assistito](#come-copilot-mi-ha-assistito). |

## Funzionalità

- **Elenco eventi** con ricerca testuale (con *debounce*), filtro per categoria e "nascondi eventi conclusi"; lista virtualizzata.
- **Dettaglio evento** con modifica in linea tramite Event Card (binding bidirezionale) e indicatori di capienza/presenze.
- **Registrazione** con convalida lato client (DataAnnotations) e regole di business: evento concluso, evento al completo, email già registrata.
- **Sessione utente**: accesso con nome ed email, precompilazione dei moduli, pagina "Le mie iscrizioni", logout; la sessione è salvata in `sessionStorage` e scade dopo 4 ore.
- **Monitoraggio presenze**: per ogni evento, elenco iscritti, check-in/annullamento, iscritti/capienza, presenti e tasso di presenza.
- **Persistenza nel browser**: iscrizioni e presenze in `localStorage`, quindi sopravvivono al ricaricamento della pagina.

## Tecnologie

- .NET 10, Blazor WebAssembly standalone, Bootstrap 5
- xUnit per i test unitari
- GitHub Actions per la CI e per il deploy su GitHub Pages

## Avvio rapido

Prerequisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/cgiuseppe8786/EventEase.git
cd EventEase
dotnet run --project src/EventEase
```

Poi apri <http://localhost:5114>.

### Test

```bash
dotnet test
```

I test (48) coprono la convalida dei modelli, il servizio eventi (cache e ordinamento), le registrazioni e i check-in,
la sessione utente (scadenza, ripristino, logout) e la protezione da *open redirect* del parametro `returnUrl`.

## Rotte

| Rotta | Pagina |
|---|---|
| `/` | Home: prossimi eventi, statistiche, categorie |
| `/events` | Elenco eventi (`?category=...`, `?q=...`) |
| `/events/{Id:int}` | Dettaglio e modifica di un evento |
| `/events/{EventId:int}/register` | Modulo di registrazione |
| `/my-events` | Iscrizioni dell'utente in sessione |
| `/attendance`, `/attendance/{EventId:int}` | Monitoraggio presenze |
| `/login?returnUrl=...` | Avvio della sessione utente |
| qualsiasi altra rotta | Pagina 404 |

## Struttura del progetto

```text
EventEase.slnx
├── src/EventEase/
│   ├── Components/      EventCard, CapacityBar, EventNotFound, LoadingIndicator
│   ├── Helpers/         formattazione date in italiano, validazione del returnUrl
│   ├── Layout/          MainLayout (sessione + ErrorBoundary), NavMenu
│   ├── Models/          EventItem, RegistrationModel, LoginModel, Attendee, UserSession
│   ├── Pages/           Home, Events, EventDetails, Register, MyEvents, Attendance, Login, NotFound
│   ├── Services/        EventService, AttendanceService, UserSessionService, BrowserStorage
│   ├── Validation/      FutureDate, MustBeTrue, pattern per nome ed email
│   └── wwwroot/sample-data/events.json
└── tests/EventEase.Tests/
```

## Debug e ottimizzazioni

### Errori di routing corretti

- **Dati non aggiornati cambiando evento**: navigando da `/events/1` a `/events/3` Blazor riusa la stessa istanza della pagina, quindi `OnInitializedAsync` non viene richiamato. Il caricamento è stato spostato in `OnParametersSetAsync` (dettaglio, registrazione, presenze, filtri da query string).
- **Id non numerici o inesistenti**: il vincolo `{Id:int}` invia `/events/abc` alla pagina 404, mentre `/events/99` mostra "Evento non trovato" invece di un'eccezione.
- **Modifica "orfana"**: se si cambia evento mentre la card è in modifica, la bozza dell'evento precedente viene scartata.
- **Open redirect**: il parametro `returnUrl` della pagina di accesso accetta solo percorsi locali ([UrlHelper.cs](src/EventEase/Helpers/UrlHelper.cs)).
- **Categoria sconosciuta nella query string**: viene ignorata invece di produrre un elenco vuoto.

### Convalida degli input

- `[EmailAddress]` accettava indirizzi come `mario@localhost`: sostituito da un'espressione regolare più rigorosa.
- Un campo telefono svuotato produceva `""`, che `[Phone]` rifiuta anche se il campo è facoltativo: i valori vuoti vengono normalizzati a `null`.
- Consenso obbligatorio con un attributo personalizzato `[MustBeTrue]` (`[Required]` non funziona sui `bool`).
- Data evento nel futuro (`[FutureDate]`) e capienza mai inferiore agli iscritti già presenti (regola aggiunta all'`EditContext` della card).
- Email confrontate senza distinzione tra maiuscole e minuscole e spazi rimossi, per evitare registrazioni duplicate.
- Pulsante di invio disabilitato durante l'elaborazione, per evitare doppi invii.

### Prestazioni e robustezza

- `Virtualize` per l'elenco eventi e per la tabella delle presenze: vengono renderizzati solo gli elementi visibili.
- Ricerca con *debounce* (300 ms): il filtro non viene ricalcolato a ogni tasto, ma quando l'utente smette di scrivere.
- `@key` sugli elementi delle liste, per un *diffing* efficiente.
- `EventService` scarica il JSON una sola volta e condivide lo stesso `Task` tra componenti concorrenti; in caso di errore non memorizza il fallimento e riprova alla chiamata successiva.
- `CapacityBar` riceve solo parametri primitivi, così Blazor salta i render quando i valori non cambiano; la percentuale è un intero per evitare CSS non valido con culture che usano la virgola decimale.
- `ErrorBoundary` nel layout, con ripristino automatico alla navigazione; lettura dello storage tollerante a dati corrotti o storage disabilitato.

## Come Copilot mi ha assistito

**1. Impostazione e componente Event Card (Attività 1).** Ho chiesto a Copilot di generare un componente Blazor per mostrare
un evento con nome, data e luogo. Copilot ha proposto il modello `EventItem` e un primo `EventCard.razor`; poi gli ho chiesto
come rendere il binding bidirezionale e mi ha suggerito la coppia `Event` / `EventChanged` per usare `@bind-Event`,
più una copia di lavoro per la modifica, così che "Annulla" non alteri i dati originali.

**2. Routing.** Copilot ha generato le pagine con le direttive `@page` e i parametri di rotta. Quando passando da un evento
all'altro la pagina continuava a mostrare i dati precedenti, mi ha spiegato che il componente viene riutilizzato e mi ha
suggerito di spostare il caricamento in `OnParametersSetAsync`. Mi ha anche aiutato ad aggiungere i vincoli `:int`,
la pagina 404 e il messaggio per gli eventi inesistenti.

**3. Debug e ottimizzazione (Attività 2).** Ho usato Copilot Chat per analizzare i casi limite della convalida: ha individuato
che `[EmailAddress]` accetta `mario@localhost` e che un telefono svuotato fa fallire `[Phone]`, proponendo una regex e la
normalizzazione a `null`. Per le prestazioni ha suggerito `Virtualize`, il *debounce* della ricerca, `@key` e la cache
del caricamento dei dati. Ha anche segnalato il rischio di *open redirect* sul `returnUrl`.

**4. Funzionalità avanzate (Attività 3).** Copilot ha generato la struttura del modulo di registrazione con `EditForm`,
`DataAnnotationsValidator` e messaggi in italiano, e mi ha suggerito un `ValidationMessageStore` per mostrare sul campo email
l'errore "già registrato". Ha proposto i servizi per la sessione utente (con evento `OnChange` per aggiornare l'interfaccia
e salvataggio in `sessionStorage`) e per le presenze (check-in, statistiche, persistenza in `localStorage`).

**5. Test e documentazione.** Copilot ha generato la base dei test xUnit con oggetti fittizi (storage in memoria,
`TimeProvider` controllabile, handler HTTP finto), il workflow di GitHub Actions e la prima stesura di questo README.
Ho rivisto ogni suggerimento, eseguito i test e verificato manualmente i flussi nel browser.

## Note

- È una demo senza backend: gli eventi vengono letti da `wwwroot/sample-data/events.json`; le modifiche agli eventi restano in memoria fino al ricaricamento, mentre iscrizioni e presenze sono salvate nel browser.
- L'accesso è dimostrativo (nome + email, senza password) e serve a mostrare la gestione dello stato della sessione.
