# Guía de música de fondo — Valkor

## Cómo está organizado el juego

Tu juego usa DOS escenas principales:

- **Test.unity** → contiene el menú host/cliente + la fase de construcción + la fase de combate
- **GameLost.unity** → pantalla de resultado (victoria o derrota)

Dentro de Test.unity la música cambia en tres momentos:
1. Al cargar la escena → música de menú
2. Cuando arranca el contador de construcción → música de construcción
3. Cuando termina el contador → música de combate

---

## PASO 1 — Consigue las pistas de audio

Necesitas entre 3 y 4 archivos de audio. Puedes descargarlos gratis en:
- https://freesound.org
- https://pixabay.com/music
- https://mixkit.co/free-music-tracks

Los archivos que necesitas:

| Nombre sugerido          | Cuándo suena                            | Estilo sugerido         |
|--------------------------|-----------------------------------------|-------------------------|
| `musica_menu.mp3`        | Pantalla de host/cliente                | Tranquilo, ambiental    |
| `musica_construccion.mp3`| Durante los 30s de construcción         | Tensión media           |
| `musica_combate.mp3`     | Desde que acaba construcción hasta fin  | Acción, intensidad      |
| `musica_victoria.mp3`    | Pantalla de resultado si ganaste        | Épico, alegre           |
| `musica_derrota.mp3`     | Pantalla de resultado si perdiste       | Dramático, triste       |

---

## PASO 2 — Importar los archivos a Unity

1. Abre Unity con tu proyecto Valkor
2. En el panel **Project** (abajo), navega hasta `Assets/`
3. Haz clic derecho sobre la carpeta `Assets` → **Create → Folder**
4. Llámala `Audio`
5. Abre esa carpeta `Audio` recién creada
6. Desde el explorador de tu ordenador, arrastra todos tus archivos `.mp3` o `.wav`
   dentro de esa carpeta en el panel Project de Unity
7. Selecciona CADA archivo en el Project, y en el Inspector (derecha) comprueba:
   - **Load Type** → `Compressed In Memory`
   - **Loop** → activa la casilla ✓ (solo para las músicas de fondo, no para efectos)
8. Pulsa **Apply** en cada uno

---

## PASO 3 — Configurar la música en la escena Test.unity

### 3a — Abrir la escena

En el panel Project navega a `Assets/@Assets/` y haz doble clic en **Test.unity**

### 3b — Crear el objeto de música

1. En el panel **Hierarchy** (izquierda), haz clic derecho en un espacio vacío
2. Selecciona **Create Empty**
3. Se creará un objeto llamado `GameObject`. Renómbralo a `GameMusicManager`
   (haz clic sobre el nombre en el Inspector y escribe el nuevo nombre)

### 3c — Añadir los componentes

Con `GameMusicManager` seleccionado en la jerarquía:

1. En el Inspector, haz clic en **Add Component**
2. Escribe `Audio Source` y selecciónalo
3. En el componente **Audio Source** que aparece:
   - Desactiva la casilla **Play On Awake** (queremos que el script controle cuándo empieza)
   - Activa la casilla **Loop**
   - En **Spatial Blend** pon `0` (para que la música se oiga igual desde cualquier punto del mapa)
4. Vuelve a hacer clic en **Add Component**
5. Escribe `Game Music Manager` y selecciónalo

### 3d — Asignar las pistas

Con `GameMusicManager` seleccionado, en el Inspector verás el componente
**Game Music Manager** con tres campos:

- **Musica Menu** → arrastra `musica_menu.mp3` desde el panel Project aquí
- **Musica Construccion** → arrastra `musica_construccion.mp3`
- **Musica Combate** → arrastra `musica_combate.mp3`

### 3e — Añadir Audio Source al BuildPhaseTimer

El BuildPhaseTimer necesita su propio AudioSource para los efectos de tick
(son efectos de sonido cortos, separados de la música de fondo).

1. En la jerarquía busca el objeto que tiene el componente **Build Phase Timer**
   (busca en la jerarquía, probablemente se llame algo como `GameManager` o `Timer`)
2. Selecciónalo → **Add Component → Audio Source**
3. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → `0`
4. En el componente **Build Phase Timer** verás dos campos de audio:
   - **Tick Clip** → arrastra un sonido de tick suave (opcional)
   - **Urgent Tick Clip** → arrastra un sonido de tick urgente (opcional)

### 3f — Guardar la escena

Pulsa **Ctrl + S**

---

## PASO 4 — Configurar la música en la escena GameLost.unity

Esta escena ya tiene un script llamado `GameEndAudioController` que diferencia
victoria y derrota automáticamente. Solo tienes que asignarle las pistas.

1. En el panel Project navega a `Assets/Scenes/` y abre **GameLost.unity**
2. En la jerarquía busca el objeto que tiene el componente **Game End Audio Controller**
3. Selecciónalo. En el Inspector verás dos campos:
   - **Win Audio** → arrastra `musica_victoria.mp3`
   - **Lose Audio** → arrastra `musica_derrota.mp3`
4. Comprueba que ese mismo objeto tiene un **Audio Source** con **Play On Awake** desactivado
   (el script lo activa él solo al cargar la escena)
5. Pulsa **Ctrl + S**

---

## Cómo funciona todo junto

```
Test.unity carga
    │
    ▼
GameMusicManager.Awake()  →  suena musica_menu  (en bucle)
    │
    │  Los dos jugadores se conectan
    │  BuildPhaseTimer arranca el contador
    ▼
BuildPhaseTimer llama CambiarAConstruccion()  →  suena musica_construccion  (en bucle)
    │
    │  Pasan los 30 segundos
    ▼
BuildPhaseTimer llama CambiarACombate()  →  suena musica_combate  (en bucle)
    │
    │  Un jugador lleva la bandera a su base
    ▼
Se carga GameLost.unity
    │
    ▼
GameEndAudioController  →  suena musica_victoria  o  musica_derrota
```

---

## Resumen de qué archivo tocar si algo no funciona

| Problema                                 | Archivo a revisar                                         |
|------------------------------------------|-----------------------------------------------------------|
| No suena nada en el menú                 | En la escena Test.unity, objeto GameMusicManager          |
| No cambia a música de construcción       | BuildPhaseTimer.cs o que GameMusicManager esté en escena  |
| No cambia a música de combate            | BuildPhaseTimer.cs o que GameMusicManager esté en escena  |
| No suena nada en la pantalla de resultado| GameLost.unity, objeto con GameEndAudioController         |
| La música no hace loop                   | Selecciona el .mp3 en Project → Inspector → Loop ✓ Apply  |
