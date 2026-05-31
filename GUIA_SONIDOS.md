# Guía de implementación de sonidos — Valkor

Todos los scripts ya están modificados. Esta guía explica qué hacer en el Editor de Unity
para que los sonidos funcionen en cada caso.

---

## Paso previo — Preparar tus archivos de audio

1. Crea la carpeta `Assets/@Assets/Audio/SFX/`
2. Arrastra ahí todos tus archivos `.wav` o `.mp3`
3. Selecciona cada uno en el Inspector y comprueba que:
   - **Load Type** → `Decompress On Load` (para clips cortos de SFX)
   - **Compression Format** → `Vorbis` (buena relación calidad/tamaño)

---

## 1. DestructibleWall — Golpe y destrucción de pared

**Script modificado:** `Assets/@Assets/Multiplayer/Scripts/DestructibleWall.cs`

**¿Dónde está este objeto?**
Es un prefab que se spawnea en red. Búscalo en el Project, probablemente en
`Assets/@AngelAssets/` o en la escena `Test.unity` como objeto prefabricado de defensa.

**Pasos en el Editor:**

1. Localiza el prefab de la pared destructible en el Project panel
2. Haz doble clic para abrirlo en modo Prefab (o selecciónalo en la escena)
3. En el Inspector → **Add Component → Audio Source**
4. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → pon `1` (sonido 3D, se oirá más fuerte cuanto más cerca estés)
5. En el componente `Destructible Wall`, aparecerán dos campos nuevos:
   - **Hit Clip** → arrastra tu sonido de golpe (ej. `impact_wood.wav`)
   - **Destroy Clip** → arrastra tu sonido de explosión/destrucción

---

## 2. VRHandAttack — Puñetazo del jugador

**Script modificado:** `Assets/@Assets/Multiplayer/Scripts/VRHandAttack.cs`

**¿Dónde está este objeto?**
En los prefabs del jugador: `Assets/Prefabs/Player.prefab` y `Assets/Prefabs/Player 2.prefab`.
Dentro del prefab, busca el GameObject hijo que representa **la mano** (el que tiene el Collider
de trigger para el puñetazo y el componente `VRHandAttack`).

**Pasos en el Editor:**

1. Abre `Assets/Prefabs/Player.prefab`
2. En la jerarquía del prefab, localiza el GameObject de la mano con `VRHandAttack`
3. Selecciónalo → **Add Component → Audio Source**
4. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → `1` (3D)
5. En el componente `VRHandAttack`:
   - **Punch Clip** → arrastra tu sonido de golpe (ej. `punch_hit.wav`)
6. Repite lo mismo en `Player 2.prefab`

---

## 3. PlayerMovementManager — Sprint, salto y sin stamina

**Script modificado:** `Assets/Scripts/PlayerMovementManager.cs`

**¿Dónde está este objeto?**
En los prefabs del jugador: `Assets/Prefabs/Player.prefab` y `Assets/Prefabs/Player 2.prefab`.
El componente `PlayerMovementManager` está en el **root** del prefab (el GameObject principal).

**Pasos en el Editor:**

1. Abre `Assets/Prefabs/Player.prefab`
2. Selecciona el GameObject raíz (el que tiene `PlayerMovementManager`)
3. **Add Component → Audio Source**
4. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → `0` (sonido 2D, es el jugador local, no necesita posición 3D)
5. En el componente `Player Movement Manager` verás 4 campos nuevos:
   - **Sprint Start Clip** → sonido de activar sprint (ej. `sprint_start.wav`)
   - **Sprint Stop Clip** → sonido de desactivar sprint (ej. `footstep_slow.wav`)
   - **Jump Clip** → sonido de salto (ej. `jump.wav`)
   - **Stamina Empty Clip** → sonido de quedarse sin stamina (ej. `exhausted_breath.wav`)
6. Repite en `Player 2.prefab`

---

## 4. BuildPhaseTimer — Temporizador de fase de construcción

**Script modificado:** `Assets/@Assets/Multiplayer/Scripts/BuildPhaseTimer.cs`

**¿Dónde está este objeto?**
Es un objeto en la **escena** `Assets/@Assets/Test.unity` (no en un prefab).
Búscalo en la jerarquía de la escena — tendrá el componente `Build Phase Timer`.

**Pasos en el Editor:**

1. Abre la escena `Test.unity`
2. En la jerarquía busca el GameObject con `Build Phase Timer`
3. Selecciónalo → **Add Component → Audio Source**
4. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → `0` (2D, es música de UI)
5. En el componente `Build Phase Timer` verás 4 campos nuevos:
   - **Phase Start Clip** → fanfare o sonido de inicio (ej. `build_start.wav`)
   - **Tick Clip** → tick suave cada segundo (ej. `tick_soft.wav`)
   - **Urgent Tick Clip** → tick urgente para los últimos 5s (ej. `tick_urgent.wav`)
   - **Phase End Clip** → bocina o señal de fin (ej. `phase_end.wav`)

---

## 5. NetworkGrabbable — Agarrar la bandera

**Script modificado:** `Assets/Scripts/networkGrabbable.cs`

**¿Dónde está este objeto?**
En los prefabs de las banderas:
- `Assets/@AngelAssets/Multiplayer/Prefabs/BanderaP1.prefab`
- `Assets/@AngelAssets/Multiplayer/Prefabs/BanderaP2.prefab`

**Pasos en el Editor:**

1. Abre `BanderaP1.prefab`
2. Selecciona el GameObject raíz → **Add Component → Audio Source**
3. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → `1` (3D, la bandera tiene posición en el mundo)
4. En el componente `Network Grabbable`:
   - **Grab Clip** → sonido al agarrar la bandera (ej. `flag_grab.wav`)
5. Repite en `BanderaP2.prefab`

---

## 6. FlagStandController — Captura de bandera (victoria)

**Script modificado:** `Assets/Scripts/FlagStandController.cs`

**¿Dónde está este objeto?**
En los prefabs de los pedestales de bandera:
- `Assets/@AngelAssets/Multiplayer/Prefabs/FlagStandP1.prefab`
- `Assets/@AngelAssets/Multiplayer/Prefabs/FlagStandP2.prefab`

**Pasos en el Editor:**

1. Abre `FlagStandP1.prefab`
2. Selecciona el GameObject raíz → **Add Component → Audio Source**
3. En el Audio Source:
   - Desactiva **Play On Awake**
   - **Spatial Blend** → `1` (3D)
4. En el componente `Flag Stand Controller`:
   - **Flag Captured Clip** → sonido épico de victoria (ej. `victory_fanfare.wav`)
5. Repite en `FlagStandP2.prefab`

---

## Resumen de sonidos que necesitas conseguir

| Clip                | Dónde se usa                        | Tipo sugerido              |
|---------------------|-------------------------------------|----------------------------|
| `hitClip`           | DestructibleWall                    | Impacto madera/piedra      |
| `destroyClip`       | DestructibleWall                    | Explosión/derrumbe         |
| `punchClip`         | VRHandAttack                        | Golpe seco                 |
| `sprintStartClip`   | PlayerMovementManager               | Pisada rápida / arranque   |
| `sprintStopClip`    | PlayerMovementManager               | Frenazo / respiración      |
| `jumpClip`          | PlayerMovementManager               | Salto                      |
| `staminaEmptyClip`  | PlayerMovementManager               | Respiración agotada        |
| `phaseStartClip`    | BuildPhaseTimer                     | Fanfare de inicio          |
| `tickClip`          | BuildPhaseTimer                     | Tick suave                 |
| `urgentTickClip`    | BuildPhaseTimer                     | Tick urgente               |
| `phaseEndClip`      | BuildPhaseTimer                     | Señal de fin               |
| `grabClip`          | NetworkGrabbable (banderas)         | Sonido al agarrar          |
| `flagCapturedClip`  | FlagStandController                 | Victoria / fanfare         |

Puedes encontrar sonidos gratuitos en: **freesound.org**, **mixkit.co** o **pixabay.com/music**

---

## Nota sobre sonidos en red

Los sonidos del jugador (sprint, salto, stamina) **solo suenan localmente** — son sonidos
que solo escucha el jugador local, lo cual es correcto para VR.

Los sonidos de las paredes (`OnHealthChanged`) ya se sincronizan automáticamente porque
`NetworkVariable` dispara el callback en todos los clientes al cambiar.

El sonido de destrucción usa un RPC propio (`PlayDestroyAudioRpc`) para asegurarse de que
suena en todos los clientes antes de que el objeto desaparezca de la red.
