# Design Document - Gioco del Ladro d'Arte

## Overview

Il Gioco del Ladro d'Arte è un'esperienza narrativa interattiva 2D/2.5D che combina meccaniche stealth, puzzle artistici e memoria visiva. Il giocatore impersona un ladro d'arte che deve completare missioni strutturate in quattro fasi sequenziali: Osservazione, Creazione, Scambio e Rivelazione. Ogni missione ruota attorno a un'opera d'arte famosa che il giocatore deve studiare, riprodurre e sostituire con un falso.

Il gioco è progettato per essere educativo e coinvolgente, integrando informazioni storiche sull'arte attraverso un sistema di Mentore IA che guida il giocatore durante l'esperienza.

## Architecture

### High-Level Architecture

Il gioco segue un'architettura modulare basata su scene, dove ogni fase della missione è una scena indipendente con meccaniche specifiche. La struttura è organizzata come segue:

```
Game Core
├── Mission Manager (gestione progressione missioni)
├── Scene Manager (transizioni tra fasi)
├── Save System (persistenza dati)
└── Game State (stato globale del gioco)

Gameplay Modules
├── Stealth Module (Osservazione e Scambio)
├── Creation Module (Creazione falso)
├── Revelation Module (Cutscene interattiva)
└── Gallery Module (visualizzazione collezione)

Support Systems
├── AI Mentor System
├── Scoring System (Punteggio_Fedeltà)
├── Difficulty Scaler
└── Art Style Manager
```

### Technology Stack Recommendation

- **Engine**: Unity 2D o Godot (per supporto 2D/2.5D nativo)
- **Linguaggio**: C# (Unity) o GDScript (Godot)
- **Asset Management**: Sistema di asset bundles per opere d'arte e ambienti
- **Persistenza**: JSON per salvataggi locali
- **Audio**: Sistema audio integrato dell'engine per musica e effetti sonori

**Rationale**: Unity/Godot offrono strumenti robusti per giochi 2D con supporto nativo per animazioni, fisica e gestione scene. La scelta tra i due dipende dalle preferenze del team e dalla necessità di deployment multipiattaforma.

## Components and Interfaces

### 1. Mission Manager

**Responsabilità**: Gestisce la progressione delle missioni e lo sblocco delle fasi.

**Interfaccia**:
```csharp
interface IMissionManager {
    Mission GetCurrentMission();
    void StartMission(string missionId);
    void CompletePhase(PhaseType phase);
    bool IsPhaseUnlocked(PhaseType phase);
    void UnlockNextMission();
    List<Mission> GetCompletedMissions();
}
```

**Dati**:
- Lista di missioni disponibili
- Stato di completamento per ogni fase
- Punteggi e statistiche per missione

### 2. Stealth Module

**Responsabilità**: Gestisce le meccaniche stealth per le fasi Osservazione e Scambio.

**Componenti**:
- **Guard AI**: Sistema di pattugliamento con waypoint predefiniti
- **Vision Cone System**: Visualizzazione campo visivo guardie
- **Detection System**: Calcolo rilevamento giocatore (timer 2 secondi)
- **Player Controller**: Movimento e interazione con ambiente

**Interfaccia**:
```csharp
interface IStealthModule {
    void InitializeLevel(LevelData levelData);
    void SpawnGuards(int guardCount, PatrolPattern[] patterns);
    bool IsPlayerDetected();
    void ActivateSpecialTool(ToolType tool);
    void OnArtworkReached();
}
```

**Design Decision**: Il sistema di detection usa un timer di 2 secondi per dare al giocatore la possibilità di uscire dal campo visivo prima del fallimento. I coni di visione sono implementati con raycast per performance ottimali.

### 3. Observation System

**Responsabilità**: Gestisce l'analisi interattiva dell'opera durante la fase Osservazione.

**Componenti**:
- **Artwork Viewer**: Interfaccia per visualizzare l'opera in dettaglio
- **Detail Highlighter**: Sistema per evidenziare aree specifiche
- **Timer**: Contatore minimo 30 secondi di osservazione
- **AI Mentor Integration**: Fornisce 3-5 informazioni testuali

**Interfaccia**:
```csharp
interface IObservationSystem {
    void DisplayArtwork(Artwork artwork);
    void HighlightDetail(DetailPoint point);
    void RecordObservationTime(float seconds);
    bool IsMinimumTimeReached();
    List<string> GetMentorInsights();
}
```

### 4. Creation Module

**Responsabilità**: Gestisce la creazione del falso attraverso quiz e disegno.

**Componenti**:
- **Quiz System**: Presenta 5-8 domande a risposta multipla
- **Drawing Canvas**: Interfaccia per tracciare contorni
- **Scoring Calculator**: Calcola Punteggio_Fedeltà
- **Fake Generator**: Genera versione stilizzata del falso

**Interfaccia**:
```csharp
interface ICreationModule {
    void StartQuiz(List<Question> questions);
    void SubmitAnswer(int questionId, int answerId);
    int CalculateQuizScore();
    void InitializeCanvas(Artwork reference);
    void EvaluateDrawing(List<Vector2> drawnPoints);
    int CalculateFidelityScore(int quizScore, int drawingScore);
    Sprite GenerateFake(int fidelityScore);
}
```

**Design Decision**: Il sistema di disegno usa una semplificazione basata su tracciamento di contorni principali piuttosto che pixel-perfect painting, per mantenere l'accessibilità e ridurre la frustrazione. Il Punteggio_Fedeltà è calcolato come somma pesata: 60% quiz + 40% disegno.

### 5. Special Tools System

**Responsabilità**: Gestisce gli strumenti speciali utilizzabili nella fase Scambio.

**Strumenti Implementati**:

#### Melodia del Sonno (Rhythm Minigame)
- **Meccanica**: Minigame ritmico con spartito musicale
- **Durata**: 15-20 secondi
- **Soglia successo**: 70% precisione
- **Effetto**: Addormenta guardie in raggio 5 metri per 30 secondi

#### Distrazione Fuoco (Microphone Input)
- **Meccanica**: Input vocale dal microfono
- **Durata**: 3 secondi di registrazione
- **Soglia successo**: Volume sopra threshold definito
- **Effetto**: Attira guardie lontano per 20 secondi

**Interfaccia**:
```csharp
interface ISpecialToolsSystem {
    void ActivateTool(ToolType tool);
    bool IsToolAvailable(ToolType tool);
    void StartRhythmMinigame(MusicSheet sheet);
    float EvaluateRhythmAccuracy();
    void StartMicrophoneInput();
    bool CheckVolumeThreshold(float[] audioSamples);
    void ApplyToolEffect(ToolType tool, Vector2 position);
}
```

**Design Decision**: Gli strumenti sono monouso per missione per mantenere la sfida. Il rhythm minigame usa un sistema di note che scorrono simile a Guitar Hero, mentre l'input microfono richiede permessi runtime su mobile.

### 6. Revelation System

**Responsabilità**: Gestisce la cutscene finale con valutazione del curatore.

**Componenti**:
- **Dialogue System**: Presenta dialoghi basati su Punteggio_Fedeltà
- **Curator AI**: Genera reazioni appropriate
- **Outcome Evaluator**: Determina successo/fallimento

**Logica di Valutazione**:
- **< 60**: Fallimento - Curatore scopre il falso
- **60-85**: Successo Parziale - Curatore ha dubbi ma accetta
- **> 85**: Successo Perfetto - Curatore completamente convinto

**Interfaccia**:
```csharp
interface IRevelationSystem {
    void StartCutscene(int fidelityScore);
    string GenerateCuratorDialogue(int score);
    MissionOutcome EvaluateOutcome(int score);
    void UnlockNextMission();
}
```

### 7. AI Mentor System

**Responsabilità**: Fornisce assistenza contestuale e informazioni culturali.

**Funzionalità per Fase**:
- **Osservazione**: Suggerimenti su dettagli non analizzati
- **Creazione**: Indizi per quiz (max 2 per missione)
- **Scambio**: Evidenziazione pattern guardie (5 secondi)
- **Generale**: Aneddoti storici durante caricamenti

**Interfaccia**:
```csharp
interface IAIMentorSystem {
    string GetHint(PhaseType phase, HintContext context);
    void HighlightGuardPattern(Guard guard, float duration);
    List<string> GetHistoricalAnecdotes(Artwork artwork);
    bool CanProvideHint(PhaseType phase);
    void ConsumeHint();
}
```

**Design Decision**: Il sistema limita gli indizi per mantenere la sfida, ma fornisce sempre informazioni culturali illimitate per l'aspetto educativo.

### 8. Gallery System

**Responsabilità**: Visualizza la collezione personale del giocatore.

**Componenti**:
- **Gallery Viewer**: Interfaccia griglia per opere
- **Comparison View**: Affianca falso e originale
- **Statistics Display**: Mostra metriche dettagliate

**Dati Visualizzati**:
- Immagine falso vs originale
- Punteggio_Fedeltà
- Stato missione (perfetto/parziale/fallimento)
- Tempo impiegato
- Numero tentativi
- Informazioni storiche complete

**Interfaccia**:
```csharp
interface IGallerySystem {
    void DisplayGallery();
    List<CompletedArtwork> GetCompletedArtworks();
    void ShowArtworkDetails(string artworkId);
    ArtworkStatistics GetStatistics(string artworkId);
}
```

### 9. Difficulty Scaler

**Responsabilità**: Aumenta progressivamente la difficoltà del gioco.

**Scaling Rules**:
- **Guardie**: +1-2 ogni 3 missioni completate
- **Tempo Osservazione**: -5 secondi ogni 2 missioni (minimo 20s)
- **Complessità Quiz**: Domande più dettagliate dopo missione 5
- **Durata Strumenti**: -20% efficacia dopo missione 7

**Interfaccia**:
```csharp
interface IDifficultyScaler {
    int CalculateGuardCount(int missionNumber);
    float CalculateObservationTime(int missionNumber);
    DifficultyLevel GetQuizDifficulty(int missionNumber);
    float GetToolEffectiveness(int missionNumber);
}
```

**Design Decision**: La progressione è graduale per evitare spike di difficoltà. I valori minimi garantiscono che il gioco rimanga giocabile anche nelle missioni avanzate.

### 10. Art Style Manager

**Responsabilità**: Applica filtri visivi e palette coerenti con le correnti artistiche.

**Componenti**:
- **Shader System**: Applica post-processing per stili artistici
- **Color Palette Manager**: Gestisce palette per periodo storico
- **Lighting Controller**: Adatta illuminazione per atmosfera

**Stili Supportati**:
- Impressionismo (pennellate visibili, colori vivaci)
- Espressionismo (contrasti forti, forme distorte)
- Cubismo (geometrie frammentate)
- Realismo (dettagli precisi)
- Altri stili classici

**Interfaccia**:
```csharp
interface IArtStyleManager {
    void ApplyArtStyle(ArtMovement movement);
    void SetColorPalette(HistoricalPeriod period);
    void ConfigureGalleryLighting();
}
```

**Design Decision**: L'uso di shader programmabili permette di cambiare drasticamente l'aspetto visivo senza duplicare asset. La Galleria usa illuminazione neutra per non alterare i colori dei falsi.

## Data Models

### Mission
```csharp
class Mission {
    string id;
    string artworkName;
    string artist;
    ArtMovement artMovement;
    HistoricalPeriod period;
    Sprite artworkImage;
    List<DetailPoint> observationPoints;
    List<Question> quizQuestions;
    LevelData observationLevel;
    LevelData swapLevel;
    int requiredMissionNumber; // Per unlock progressivo
    bool isCompleted;
}
```

### PhaseProgress
```csharp
class PhaseProgress {
    PhaseType phase;
    bool isCompleted;
    bool isUnlocked;
    float timeSpent;
    int attempts;
}
```

### CompletedArtwork
```csharp
class CompletedArtwork {
    string missionId;
    Sprite fakeImage;
    Sprite originalImage;
    int fidelityScore;
    MissionOutcome outcome;
    float totalTime;
    int totalAttempts;
    DateTime completionDate;
    Dictionary<PhaseType, PhaseProgress> phaseStats;
}
```

### Guard
```csharp
class Guard {
    Vector2 position;
    List<Vector2> patrolWaypoints;
    float visionRange;
    float visionAngle;
    float movementSpeed;
    GuardState currentState; // Patrol, Alert, Sleeping
}
```

### Question
```csharp
class Question {
    string questionText;
    List<string> answers;
    int correctAnswerIndex;
    QuestionDifficulty difficulty;
    string mentorHint;
}
```

### SaveData
```csharp
class SaveData {
    int saveSlot;
    string playerName;
    int currentMissionIndex;
    PhaseType currentPhase;
    List<CompletedArtwork> gallery;
    Dictionary<string, PhaseProgress> missionProgress;
    DateTime lastSaved;
}
```

## Error Handling

### Detection and Mission Failure
- **Scenario**: Giocatore rilevato da guardia
- **Handling**: 
  - Attivare animazione di allerta
  - Mostrare schermata "Missione Fallita"
  - Offrire opzioni: Riprova Fase / Torna Menu
  - Non penalizzare statistiche permanenti

### Quiz Timeout
- **Scenario**: Giocatore non risponde entro tempo limite
- **Handling**:
  - Considerare risposta errata
  - Continuare con prossima domanda
  - Ridurre Punteggio_Fedeltà proporzionalmente

### Drawing System Errors
- **Scenario**: Input non valido o canvas non inizializzato
- **Handling**:
  - Validare input prima del processing
  - Fornire feedback visivo immediato
  - Permettere reset del disegno senza penalità

### Special Tool Failures
- **Scenario**: Microfono non disponibile o rhythm minigame fallito
- **Handling**:
  - Mostrare messaggio di fallimento chiaro
  - Strumento consumato comunque (design choice per bilanciamento)
  - Suggerire approccio stealth tradizionale

### Save System Failures
- **Scenario**: Errore scrittura/lettura salvataggio
- **Handling**:
  - Tentare backup su slot alternativo
  - Notificare utente con messaggio chiaro
  - Mantenere stato in memoria fino a chiusura app
  - Log errori per debugging

### Asset Loading Failures
- **Scenario**: Immagine opera o asset mancante
- **Handling**:
  - Usare placeholder generico
  - Loggare errore per fix
  - Permettere comunque completamento missione

## Testing Strategy

### Unit Testing

**Componenti Prioritari**:
- **Scoring Calculator**: Verificare calcoli Punteggio_Fedeltà con vari input
- **Difficulty Scaler**: Testare formule scaling per tutti i range di missioni
- **Detection System**: Validare logica rilevamento e timer 2 secondi
- **Quiz System**: Verificare scoring e gestione risposte

**Framework**: NUnit (Unity) o GDScript testing (Godot)

### Integration Testing

**Scenari Critici**:
- **Phase Transitions**: Verificare che dati persistano tra fasi
- **Save/Load**: Testare salvataggio e caricamento in vari stati
- **Tool Effects**: Verificare che effetti strumenti si applicano correttamente
- **Gallery Updates**: Confermare che opere completate appaiono correttamente

### Gameplay Testing

**Focus Areas**:
- **Stealth Balance**: Verificare che pattern guardie siano sfidanti ma giusti
- **Quiz Difficulty**: Assicurare che domande siano appropriate per target audience
- **Drawing System**: Testare usabilità su touch e mouse
- **Rhythm Minigame**: Calibrare timing e feedback visivo

**Metriche da Raccogliere**:
- Tempo medio per fase
- Tasso di successo per missione
- Utilizzo strumenti speciali
- Punteggi Fedeltà medi

### Performance Testing

**Target**:
- 60 FPS su dispositivi mid-range
- Tempo caricamento scene < 2 secondi
- Memoria < 500MB su mobile

**Aree Critiche**:
- Rendering coni visione multiple guardie
- Shader post-processing per stili artistici
- Caricamento asset opere ad alta risoluzione

### Accessibility Testing

**Considerazioni**:
- Colorblind modes per coni visione
- Opzioni dimensione testo
- Alternative a input microfono per Distrazione Fuoco
- Controlli rimappabili

## Technical Considerations

### Platform Support
- **Primary**: PC (Windows/Mac/Linux)
- **Secondary**: Mobile (iOS/Android) - richiede adattamenti UI touch
- **Future**: Console (richiede controller mapping)

### Localization
- Sistema di stringhe esternalizzate per UI
- Database opere d'arte multilingua
- Dialoghi Mentore IA localizzati
- Considerare differenze culturali nella presentazione opere

### Performance Optimization
- Object pooling per guardie e proiettili vision cone
- Atlasing per sprite opere d'arte
- LOD per ambienti museo complessi
- Lazy loading per asset Gallery

### Scalability
- Sistema modulare permette aggiunta nuove missioni facilmente
- Template per creazione livelli stealth
- Editor tool per configurare pattern guardie
- Database espandibile per opere d'arte

