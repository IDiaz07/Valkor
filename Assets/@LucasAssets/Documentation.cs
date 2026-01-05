# PatrolController - Documentación Técnica

## Índice
using System;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

1. [Descripción General](#descripción-general)
2. [Arquitectura y Diseño](#arquitectura-y-diseño)
3. [Máquina de Estados](#máquina-de-estados)
4. [Configuración en Inspector](#configuración-en-inspector)
5. [Métodos Públicos y Privados](#métodos-públicos-y-privados)
6. [Dependencias](#dependencias)
7. [Flujo de Ejecución](#flujo-de-ejecución)
8. [Sistema de Combate](#sistema-de-combate)
9. [Sistema de Animaciones](#sistema-de-animaciones)
10. [Debugging y Gizmos](#debugging-y-gizmos)
11. [Consideraciones de Rendimiento](#consideraciones-de-rendimiento)
12. [Problemas Conocidos y Soluciones](#problemas-conocidos-y-soluciones)
13. [Ejemplos de Uso](#ejemplos-de-uso)

---

## Descripción General

`PatrolController` es el componente principal que gestiona el comportamiento de IA de los enemigos en el juego.Implementa una * *máquina de estados finitos(FSM) * *que controla el ciclo completo de comportamiento enemigo: patrullaje, detección, persecución, combate y retorno.

### Responsabilidades principales:
- Navegación autónoma entre puntos de patrullaje usando NavMesh
- Detección y persecución del jugador(delegada a `EnemyVisionSensor`)
- Gestión de combate cuerpo a cuerpo con sistema de golpes temporizados
- Control de animaciones según el estado actual
- Sistema de stun al recibir daño

### Versión y compatibilidad:
- **Unity * *: 6000.0.41f1(Unity 6)
- **Render Pipeline * *: URP 17.0.4
- **Input System * *: New Input System 1.13.1
- **AI Navigation * *: 2.0.6

-- -

## Arquitectura y Diseño

### Patrón de diseño: Finite State Machine (FSM)

```
┌─────────────────────────────────────────────────────────────────┐
│                        PATROL CONTROLLER                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌──────────┐    detecta     ┌──────────┐    cerca    ┌──────────┐
│   │PATROLLING│───────────────►│ CHASING  │────────────►│ FIGHTING │
│   └──────────┘                └──────────┘             └──────────┘
│        ▲                           │                        │
│        │                           │ pierde                 │ pierde
│        │      ┌──────────┐         │ vista                  │ vista
│        └──────│RETURNING │◄────────┴────────────────────────┘
│               └──────────┘
│                    │
│                    │ llega al punto
│                    ▼
│               ┌──────────┐
│               │PATROLLING│
│               └──────────┘
└─────────────────────────────────────────────────────────────────┘
```

### Diagrama de componentes:

```
┌─────────────────────┐
│   PatrolController  │
├─────────────────────┤
│ - NavMeshAgent      │◄──── Navegación
│ - Animator          │◄──── Animaciones
│ - EnemyVisionSensor │◄──── Detección (componente externo)
│ - Collider (Golpe)  │◄──── Hitbox de ataque
└─────────────────────┘
```

---

## Máquina de Estados

### Estados disponibles (enum `EnemyState`):

| Estado | Descripción | Transiciones posibles |
|--------|-------------|----------------------|
| `Patrolling` | Movimiento cíclico entre waypoints con pausas | → Chasing (detecta jugador) |
| `Chasing` | Persecución activa del jugador | → Fighting (cerca), → Returning (pierde vista) |
| `Fighting` | Combate cuerpo a cuerpo, ejecuta ataques | → Chasing (jugador se aleja), → Returning (pierde vista) |
| `Returning` | Regreso al punto de patrullaje más cercano | → Patrolling (llega), → Chasing (detecta jugador) |

### Condiciones de transición detalladas:

#### Patrolling → Chasing
```csharp
if (visionSensor.HasPlayerInSight)
    EnterChaseState();
```

#### Chasing → Fighting
```csharp
if (timeInCurrentState >= minimumChaseTime && 
    distanceToPlayer <= fightingDistance)
    EnterFightingState();
```
> **Nota**: El `minimumChaseTime` previene oscilación entre estados cuando el jugador está en el límite de distancia.

#### Fighting → Chasing
```csharp
if (distanceToPlayer > fightingStopDistance)
    EnterChaseState();
```
> **Histéresis**: `fightingStopDistance` debe ser mayor que `fightingDistance` para evitar flickering.

#### Cualquier estado → Returning
```csharp
if (!visionSensor.HasPlayerInSight)
    EnterReturningState();
```

---

## Configuración en Inspector

### Header: Configuración de Patrullaje

| Campo | Tipo | Valor por defecto | Descripción |
|-------|------|-------------------|-------------|
| `patrolPoints` | Transform[] | - | Array de waypoints. Deben ser GameObjects vacíos posicionados en la escena |
| `waitTimeAtPoint` | float | 2f | Segundos que espera en cada waypoint |
| `patrolSpeed` | float | 3f | Velocidad de movimiento durante patrullaje (unidades/segundo) |

### Header: Configuración de Persecución

| Campo | Tipo | Valor por defecto | Descripción |
|-------|------|-------------------|-------------|
| `visionSensor` | EnemyVisionSensor | null | Referencia al componente de detección. Se auto-asigna si está en el mismo GameObject |
| `chaseSpeed` | float | 5f | Velocidad durante persecución |
| `loseTargetDistance` | float | 15f | **[NO USADO ACTUALMENTE]** Distancia para perder al jugador |

### Header: Configuración de Combate

| Campo | Tipo | Valor por defecto | Descripción |
|-------|------|-------------------|-------------|
| `fightingDistance` | float | 0f | Distancia para entrar en combate |
| `fightingStopDistance` | float | 0f | Distancia para salir de combate (recomendado: > fightingDistance) |
| `fightingRotationSpeed` | float | 5f | Velocidad de rotación hacia el jugador en combate |
| `minimumChaseTime` | float | 0.5f | Tiempo mínimo persiguiendo antes de poder entrar en Fighting |
| `minTimeBetweenHits` | float | 1f | Tiempo mínimo entre ataques |
| `maxTimeBetweenHits` | float | 3f | Tiempo máximo entre ataques |
| `golpe` | Collider | null | Hitbox del ataque. Se busca automáticamente un hijo llamado "Golpe" |

### Header: Sistema de Daño

| Campo | Tipo | Valor por defecto | Descripción |
|-------|------|-------------------|-------------|
| `hitStunDuration` | float | 0.5f | Duración del stun al recibir golpe |

---

## Métodos Públicos y Privados

### Ciclo de vida de Unity

#### `void Start()`
Inicializa todos los componentes y configuraciones:
1. Obtiene referencias a `NavMeshAgent` y `Animator`
2. Configura NavMeshAgent (velocidad, autoBraking, stoppingDistance)
3. Valida y asigna `EnemyVisionSensor`
4. Inicia patrullaje si hay waypoints
5. Busca el collider "Golpe" en la jerarquía de hijos

#### `void Update()`
Loop principal ejecutado cada frame:
```
1. Verificar si hay waypoints
2. Incrementar timeInCurrentState
3. Si está stunned → manejar stun y return
4. UpdateState() → evaluar transiciones
5. Handle[Estado]() → ejecutar lógica del estado
6. UpdateAnimation() → sincronizar animaciones
```

### Gestión de estados

#### `void UpdateState()`
Evalúa las condiciones para transiciones entre estados.

**Lógica de prioridad:**
1. Si tiene jugador a la vista:
   - Desde Patrolling/Returning → Chasing
   - Desde Chasing (si tiempo suficiente y cerca) → Fighting
   - Desde Fighting (si lejos) → Chasing
2. Si NO tiene jugador a la vista:
   - Desde Chasing/Fighting → Returning

#### `void EnterChaseState()`
Configura el enemigo para persecución:
- Resetea `timeInCurrentState`
- Aumenta velocidad a `chaseSpeed`
- Activa `autoBraking`
- Desactiva `isStopped`

#### `void EnterFightingState()`
Configura el enemigo para combate:
- Detiene el NavMeshAgent (`isStopped = true`)
- Resetea timers de combate
- Calcula primer `nextHitTime` aleatorio

#### `void EnterReturningState()`
Configura retorno al patrullaje:
- Restaura `patrolSpeed`
- Encuentra waypoint más cercano
- Inicia navegación hacia él

### Lógica por estado

#### `void HandlePatrolling()`
```csharp
if (isWaiting)
    HandleWaiting();  // Contador de espera
else
    CheckArrival();   // Verificar llegada a waypoint
```

#### `void HandleChasing()`
Actualiza destino del NavMeshAgent a la posición del jugador cada frame.

#### `void HandleFighting()`
1. Verifica que el jugador sigue visible
2. Incrementa timer de combate
3. Rota suavemente hacia el jugador (Slerp)
4. Si `timeSinceEnteredFighting >= nextHitTime` → ejecuta ataque

#### `void HandleReturning()`
Verifica llegada al waypoint y transiciona a Patrolling.

### Sistema de combate

#### `void PerformHit()`
Ejecuta un ataque:
1. Dispara trigger de animación `isHitting`
2. Inicia corrutina `ManageAttackCollider()`

#### `IEnumerator ManageAttackCollider()`
Gestiona la activación temporal del hitbox:
```csharp
golpe.enabled = true;
yield return new WaitForSeconds(3f);  // Duración del ataque
golpe.enabled = false;
```

### Sistema de daño recibido

#### `void OnTriggerEnter(Collider other)`
Detecta colisiones con objetos en layer "Weapon":
```csharp
if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
    OnHitByWeapon();
```

#### `void OnHitByWeapon()`
Procesa el golpe recibido:
1. Dispara animación `beenHitted`
2. Activa estado de stun
3. Detiene NavMeshAgent

### Navegación

#### `void GoToNextPatrolPoint()`
Establece el siguiente waypoint como destino e incrementa el índice cíclicamente.

#### `int FindClosestPatrolPoint()`
Calcula y retorna el índice del waypoint más cercano a la posición actual.

#### `void CheckArrival()`
Verifica si el enemigo llegó al waypoint actual usando:
- `agent.pathPending` - path calculándose
- `agent.remainingDistance` vs `stoppingDistance`
- `agent.velocity.sqrMagnitude` - velocidad casi cero

---

## Dependencias

### Componentes requeridos en el mismo GameObject:

| Componente | Obligatorio | Notas |
|------------|-------------|-------|
| `NavMeshAgent` | ✅ Sí | Debe tener Agent Type configurado correctamente |
| `Animator` | ✅ Sí | Con Animator Controller que tenga los triggers necesarios |
| `Collider` | ⚠️ Recomendado | Para recibir daño (trigger) |
| `EnemyVisionSensor` | ✅ Sí | Puede estar en el mismo GO o asignarse manualmente |

### Componentes requeridos en hijos:

| Nombre | Componente | Notas |
|--------|------------|-------|
| "Golpe" | Collider + Rigidbody | Hitbox de ataque. Rigidbody en modo Kinematic, Collider como Trigger |

### Dependencias de escena:

- **NavMesh Surface** bakeada con el Agent Type correspondiente
- **Waypoints** (GameObjects vacíos) asignados a `patrolPoints`
- **Player** con tag "Player" (usado por EnemyVisionSensor)

---

## Flujo de Ejecución

### Diagrama de secuencia - Ciclo completo

```
Start()
   │
   ├── GetComponent<NavMeshAgent>()
   ├── GetComponent<Animator>()
   ├── Buscar EnemyVisionSensor
   ├── Buscar Collider "Golpe"
   └── GoToNextPatrolPoint()
   
Update() [cada frame]
   │
   ├── timeInCurrentState += deltaTime
   │
   ├── if (isStunned)
   │   ├── Decrementar timer
   │   ├── if (timer <= 0) → recuperar
   │   └── return
   │
   ├── UpdateState()
   │   └── Evaluar transiciones FSM
   │
   ├── switch (currentState)
   │   ├── Patrolling → HandlePatrolling()
   │   ├── Chasing → HandleChasing()
   │   ├── Fighting → HandleFighting()
   │   └── Returning → HandleReturning()
   │
   └── UpdateAnimation()
```

### Diagrama de secuencia - Ataque

```
HandleFighting()
   │
   ├── timeSinceEnteredFighting += deltaTime
   │
   ├── Rotar hacia jugador (Slerp)
   │
   └── if (time >= nextHitTime)
       │
       └── PerformHit()
           │
           ├── animator.SetTrigger("isHitting")
           │
           └── StartCoroutine(ManageAttackCollider())
               │
               ├── golpe.enabled = true
               ├── yield WaitForSeconds(3f)
               └── golpe.enabled = false
```

---

## Sistema de Combate

### Hitbox de ataque

El sistema usa un Collider hijo llamado "Golpe" que se activa/desactiva durante los ataques.

**Configuración recomendada para "Golpe":**
```
GameObject "Golpe"
├── SphereCollider (o BoxCollider)
│   ├── IsTrigger: true
│   └── Radius: ajustar según el enemigo
└── Rigidbody
    ├── IsKinematic: true
    └── UseGravity: false
```

### Timing de ataques

```
┌─────────────────────────────────────────────────────────────┐
│ Timeline de un ataque                                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ t=0        t=3s                                             │
│  │──────────│                                               │
│  │ COLLIDER │                                               │
│  │ ACTIVO   │                                               │
│  │          │                                               │
│  ▼          ▼                                               │
│ PerformHit() → WaitForSeconds(3f) → Desactivar             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

> **⚠️ Problema conocido**: El tiempo de 3 segundos está hardcodeado. Debería sincronizarse con la duración real de la animación de ataque.

### Sistema de daño recibido

El enemigo detecta golpes mediante `OnTriggerEnter` verificando el layer "Weapon".

**Flujo de daño:**
1. Weapon entra en trigger del enemigo
2. `OnTriggerEnter` verifica layer
3. `OnHitByWeapon()` aplica stun
4. Animación `beenHitted` se reproduce
5. NavMeshAgent se detiene
6. Tras `hitStunDuration` segundos, se recupera

---

## Sistema de Animaciones

### Triggers requeridos en Animator Controller

| Trigger | Cuándo se activa | Estado destino esperado |
|---------|------------------|------------------------|
| `isWalking` | Moviéndose (Patrolling, Chasing, Returning) | Walk/Run |
| `isNotWalking` | Quieto o transición | Idle |
| `isFighting` | En estado Fighting (cada frame) | Fight Idle |
| `isHitting` | Al ejecutar ataque | Attack |
| `beenHitted` | Al recibir daño | Hit Reaction |
| `die` | Al morir (gestionado por EnemyHealth) | Death |

### Diagrama de estados del Animator

```
                    ┌─────────────────┐
                    │      IDLE       │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│     WALK      │   │  FIGHT IDLE   │   │   REACTION    │
└───────┬───────┘   └───────┬───────┘   └───────────────┘
        │                   │
        │                   ▼
        │           ┌───────────────┐
        │           │    ATTACK     │
        │           └───────────────┘
        │
        └──────────────────┐
                           ▼
                    ┌───────────────┐
                    │     DEATH     │
                    └───────────────┘
```

### Notas sobre la implementación:

1. **`isFighting` se dispara cada frame** en `UpdateAnimation()` mientras está en Fighting. Esto mantiene la animación de combate activa.

2. **`wasInFightingState`** es un flag para detectar la transición de salida de Fighting y disparar `isNotWalking`.

3. **Condiciones de walking:**
   ```csharp
   bool shouldWalk = hasValidPath && 
                     isMovingToDestination && 
                     hasVelocity && 
                     !isWaiting;
   ```

---

## Debugging y Gizmos

### Visualización en Scene View

El script incluye `OnDrawGizmos()` y `OnDrawGizmosSelected()` para debugging visual:

#### Siempre visible (`OnDrawGizmos`):

| Color | Elemento | Descripción |
|-------|----------|-------------|
| 🟡 Amarillo | Esferas pequeñas + líneas | Waypoints y sus conexiones |
| 🟢 Verde | Esfera grande | Waypoint objetivo actual |
| Texto | Label sobre waypoints | Índice de cada waypoint |
| Texto | Label sobre enemigo | Estado actual |

#### Al seleccionar (`OnDrawGizmosSelected`):

| Color | Elemento | Descripción |
|-------|----------|-------------|
| 🔴 Rojo | Línea | Dirección hacia destino del NavMeshAgent |
| 🟣 Magenta | Esfera | Última posición conocida del jugador |

### Debug.Log messages

El script genera varios mensajes de debug:

```
[INFO] "¡Enemigo detectó al jugador!"
[INFO] "¡Enemigo entró en combate cuerpo a cuerpo!"
[INFO] "Entrando en Fighting. Primer golpe en: X.XXs"
[INFO] "Golpe ejecutado. Próximo golpe en: X.XXs"
[INFO] "Enemigo perdió al jugador, volviendo a patrullar"
[INFO] "Llegó al waypoint X"

[WARNING] "No hay puntos de patrullaje asignados en [nombre]"
[WARNING] "[nombre] No se encontró 'Golpe' en los hijos"

[ERROR] "No se encontró EnemyVisionSensor en [nombre]"
[ERROR] "Punto de patrullaje X es null!"

[COLORED] "<color=red>¡Enemigo golpeado por: [nombre]!</color>"
[COLORED] "<color=orange>Enemigo recibió golpe. Stunned por Xs</color>"
[COLORED] "<color=green>Enemigo recuperado del stun</color>"
```

---

## Consideraciones de Rendimiento

### Optimizaciones actuales

1. **`sqrMagnitude` en vez de `magnitude`**: Evita raíz cuadrada innecesaria para comparaciones de velocidad.

2. **Early returns**: El Update() sale temprano si no hay waypoints o está stunned.

3. **Búsqueda de "Golpe" solo en Start()**: No se busca cada frame.

### Posibles mejoras

1. **Caché de distancias**: `Vector3.Distance()` se llama múltiples veces por frame. Podría cachearse.

2. **Eventos en vez de polling**: El trigger `isFighting` se dispara cada frame. Mejor usar eventos de transición.

3. **Object pooling para corrutinas**: `ManageAttackCollider()` crea una corrutina por ataque.

4. **LOD de IA**: Reducir frecuencia de updates para enemigos lejanos.

### Métricas aproximadas

| Operación | Frecuencia | Costo |
|-----------|------------|-------|
| UpdateState() | Cada frame | Bajo (comparaciones) |
| Vector3.Distance() | 1-3 por frame | Medio (raíz cuadrada) |
| NavMeshAgent.SetDestination() | Por cambio de estado/frame en Chasing | Alto |
| Quaternion.Slerp() | Cada frame en Fighting | Bajo |

---

## Problemas Conocidos y Soluciones

### 1. Variable no usada: `loseTargetDistance`

**Problema**: Declarada pero nunca utilizada.
```csharp
[SerializeField] private float loseTargetDistance = 15f;  // NO SE USA
```

**Solución**: Implementar o eliminar. Posible uso:
```csharp
// En UpdateState(), sección de Chasing:
if (distanceToPlayer > loseTargetDistance)
{
    EnterReturningState();
}
```

### 2. Tiempo de collider hardcodeado

**Problema**: `WaitForSeconds(3f)` no coincide necesariamente con la animación.

**Solución**: Usar Animation Events o calcular desde AnimationClip:
```csharp
[SerializeField] private float attackColliderDuration = 3f;
// O mejor: Animation Event al final del ataque
```

### 3. Import no usado

**Problema**: 
```csharp
using Unity.VisualScripting.Antlr3.Runtime.Tree;  // No se usa
```

**Solución**: Eliminar el using.

### 4. Oscilación de estados

**Problema**: Si `fightingDistance` == `fightingStopDistance`, el enemigo puede oscilar.

**Solución**: Siempre configurar `fightingStopDistance > fightingDistance`:
```
Recomendado:
fightingDistance = 1.5f
fightingStopDistance = 2.5f  // Histéresis de 1 unidad
```

### 5. Input System legacy comentado

**Problema**: El test manual con `Input.GetKeyDown()` causa errores con New Input System.

**Solución**: Ya está comentado. Si se necesita test manual, usar InputAction:
```csharp
// En un script de debug separado
playerInput.actions["TestHit"].performed += ctx => enemy.TestPerformHit();
```

### 6. Collider de ataque puede quedarse activo

**Problema**: Si el enemigo muere durante un ataque, la corrutina puede no completarse.

**Solución**: Desactivar collider en muerte o usar `StopAllCoroutines()`:
```csharp
// En EnemyHealth.Die() o equivalente
GetComponent<PatrolController>()?.StopAllCoroutines();
```

---

## Ejemplos de Uso

### Configuración básica de un enemigo

```
GameObject "Slime"
├── PatrolController
│   ├── patrolPoints: [Check1, Check2, Check3, Check4]
│   ├── waitTimeAtPoint: 2
│   ├── patrolSpeed: 3
│   ├── chaseSpeed: 5
│   ├── fightingDistance: 1.5
│   ├── fightingStopDistance: 2.5
│   └── ...
├── NavMeshAgent
│   ├── Agent Type: Slime
│   └── ...
├── Animator
│   └── Controller: animator_slime
├── EnemyVisionSensor
├── EnemyHealth
├── CapsuleCollider (trigger, para recibir daño)
└── GameObject "Golpe"
    ├── SphereCollider (trigger)
    └── Rigidbody (kinematic)
```

### Crear un nuevo tipo de enemigo

1. **Crear Agent Type** en Navigation > Agents
2. **Bakear NavMesh Surface** con el nuevo Agent Type
3. **Duplicar prefab** de enemigo existente
4. **Configurar NavMeshAgent** con el Agent Type y área correctos
5. **Crear waypoints** específicos (¡no compartir con otros enemigos!)
6. **Ajustar parámetros** de combate según el enemigo

### Script de spawn de enemigos

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] patrolRouteA;
    [SerializeField] private Transform[] patrolRouteB;
    
    public void SpawnEnemy(int spawnIndex, bool useRouteA)
    {
        GameObject enemy = Instantiate(enemyPrefab, 
            spawnPoints[spawnIndex].position, 
            Quaternion.identity);
        
        PatrolController patrol = enemy.GetComponent<PatrolController>();
        
        // Asignar ruta de patrullaje vía reflexión o método público
        // (actualmente patrolPoints es privado)
    }
}
```

> **Nota**: Actualmente `patrolPoints` es privado y solo se puede asignar desde Inspector. Para spawn dinámico, habría que exponerlo con un método público o propiedad.

---

## Changelog

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | - | Implementación inicial con FSM de 4 estados |
| 1.1 | - | Añadido sistema de stun al recibir daño |
| 1.2 | - | Añadido `minimumChaseTime` para evitar oscilación |
| 1.3 | - | Sistema de golpes temporizados aleatorios |

---

## Referencias

- [Unity NavMesh Documentation](https://docs.unity3d.com/Manual/nav-NavigationSystem.html)
- [Unity Animation System](https://docs.unity3d.com/Manual/AnimationSection.html)
- [FSM Pattern in Games](https://gameprogrammingpatterns.com/state.html)

---

*Documentación generada para el proyecto Valkor*
*Última actualización: Enero 2026*






# EnemyVisionSensor - Documentación Técnica

## Índice
1. [Descripción General](#descripción-general)
2. [Arquitectura](#arquitectura)
3. [Configuración en Inspector](#configuración-en-inspector)
4. [Métodos](#métodos)
5. [Sistema de Detección](#sistema-de-detección)
6. [Dependencias](#dependencias)
7. [Debugging y Gizmos](#debugging-y-gizmos)
8. [Problemas Conocidos y Soluciones](#problemas-conocidos-y-soluciones)
9. [Optimizaciones Sugeridas](#optimizaciones-sugeridas)

---

## Descripción General

`EnemyVisionSensor` implementa un sistema de detección de jugador basado en **cono de visión** con verificación de línea de vista (raycast). Es el componente sensorial que `PatrolController` consulta para decidir transiciones de estado.

### Responsabilidades:
- Detectar jugadores dentro del rango y ángulo de visión
- Verificar que no haya obstáculos bloqueando la línea de vista
- Exponer información de detección a otros componentes
- Visualizar el cono de visión en el editor

---

## Arquitectura

### Diagrama del cono de visión
```
                    visionAngle (45°)
                         ◄───►
                          ╱╲
                         ╱  ╲
                        ╱    ╲
                       ╱      ╲
                      ╱   ●    ╲  ← Player (detectado si está aquí)
                     ╱  Player  ╲
                    ╱            ╲
        ──────────●──────────────── transform.forward
              Enemy (origin)
                    ╲            ╱
                     ╲    ●    ╱  ← Player (NO detectado, fuera del ángulo)
                      ╲      ╱
                       ╲    ╱
                        ╲  ╱
                         ╲╱
              ◄──────────────────►
                  visionRange (10)
```

### Flujo de detección
```
Update() → DetectPlayer() → Para cada Player:
    │
    ├── ¿Distancia > visionRange? → [SKIP]
    │
    ├── ¿Ángulo > visionAngle? → [SKIP]
    │
    └── Raycast hacia Player
            │
            ├── Hit es Player → detectedPlayer = player ✓
            │
            └── Hit es obstáculo → [SKIP]
```

---

## Configuración en Inspector

### Header: Configuración de Visión

| Campo | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `visionRange` | float | 10f | Radio máximo de detección (unidades) |
| `visionAngle` | float | 45f | Semi-ángulo del cono (grados desde forward) |
| `obstacleMask` | LayerMask | - | Layers que **NO** bloquean visión (invertido en código) |

> ⚠️ **Nota sobre obstacleMask**: El código usa `~obstacleMask` (negación), por lo que debes seleccionar las layers que el raycast **debe ignorar**, no las que bloquean.

### Header: Referencias

| Campo | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `eyePosition` | Transform | null | Origen del raycast. Si null, usa `transform.position + Vector3.up` |

### Propiedades Públicas (solo lectura)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `DetectedPlayer` | Transform | Transform del jugador detectado (null si ninguno) |
| `HasPlayerInSight` | bool | `true` si hay jugador detectado |

---

## Métodos

### Ciclo de Unity

| Método | Frecuencia | Descripción |
|--------|------------|-------------|
| `Update()` | Cada frame | Llama a `DetectPlayer()` |
| `OnDrawGizmosSelected()` | Editor | Visualiza cono de visión |

### Detección

#### `void DetectPlayer()`

```csharp
void DetectPlayer()
{
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    detectedPlayer = null;

    foreach (GameObject playerObj in players)
    {
        // 1. Verificar distancia
        if (distanceToPlayer > visionRange) continue;

        // 2. Verificar ángulo
        if (angleToPlayer > visionAngle) continue;

        // 3. Verificar línea de vista (raycast)
        if (Physics.Raycast(origin, direction, out hit, visionRange, ~obstacleMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                detectedPlayer = playerObj.transform;
                return;  // Salir al encontrar el primero
            }
        }
    }
}
```

**Orden de verificaciones** (de más barata a más cara):
1. **Distancia** - `Vector3.Distance()` (una raíz cuadrada)
2. **Ángulo** - `Vector3.Angle()` (producto punto + arccos)
3. **Raycast** - Más costoso, solo si pasa las anteriores

---

## Sistema de Detección

### Tres condiciones para detectar

| # | Condición | Verificación |
|---|-----------|--------------|
| 1 | **En rango** | `Vector3.Distance() <= visionRange` |
| 2 | **En ángulo** | `Vector3.Angle(forward, dirToPlayer) <= visionAngle` |
| 3 | **Línea de vista** | `Raycast` sin obstáculos hasta el Player |

### Cálculo del ángulo
```csharp
Vector3 directionToPlayer = (player.position - transform.position).normalized;
float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
```

`Vector3.Angle()` devuelve el ángulo **absoluto** (0° a 180°), por lo que `visionAngle = 45` crea un cono de **90° totales** (45° a cada lado del forward).

### Raycast y LayerMask
```csharp
// ~obstacleMask = NEGAR la máscara
// Si obstacleMask tiene "Obstacles" marcado → raycast IGNORA "Obstacles"
Physics.Raycast(origin, direction, out hit, visionRange, ~obstacleMask)
```

⚠️ **Confuso**: El nombre `obstacleMask` sugiere "layers que bloquean", pero el código lo niega.

---

## Dependencias

### Componentes requeridos

| Componente | Obligatorio | Notas |
|------------|-------------|-------|
| - | - | No requiere componentes adicionales |

### Configuración de proyecto

| Requisito | Descripción |
|-----------|-------------|
| Tag "Player" | El jugador debe tener este tag |
| Collider en Player | Para que el raycast lo detecte |

### Integración con PatrolController

```csharp
// PatrolController consulta el sensor:
if (visionSensor != null && visionSensor.HasPlayerInSight)
{
    float distance = Vector3.Distance(transform.position, 
                                       visionSensor.DetectedPlayer.position);
    // Decidir estado según distancia...
}
```

---

## Debugging y Gizmos

### Visualización en Scene View (al seleccionar)

| Color | Elemento | Condición |
|-------|----------|-----------|
| 🟡 Amarillo | Esfera + cono | Sin detección |
| 🔴 Rojo | Esfera + cono + línea | Con detección |

### Elementos dibujados

```csharp
// Esfera del rango
Gizmos.DrawWireSphere(origin, visionRange);

// Límites del cono
Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle, 0) * transform.forward * visionRange;
Vector3 rightBoundary = Quaternion.Euler(0, visionAngle, 0) * transform.forward * visionRange;
Gizmos.DrawLine(origin, origin + leftBoundary);
Gizmos.DrawLine(origin, origin + rightBoundary);

// Línea al jugador detectado
if (HasPlayerInSight)
    Gizmos.DrawLine(origin, DetectedPlayer.position);
```

### Cómo verificar funcionamiento

1. Seleccionar enemigo en Scene View
2. Cono amarillo visible
3. Mover Player dentro del cono → cono se vuelve rojo + línea hacia Player
4. Poner obstáculo entre ellos → cono vuelve a amarillo

---

## Problemas Conocidos y Soluciones

### 1. ⚠️ `FindGameObjectsWithTag` cada frame

**Problema**: Busca TODOS los objetos con tag "Player" cada frame. Costoso.

**Impacto**: Bajo si hay pocos jugadores, pero escala mal.

**Solución** - Cachear referencia:
```csharp
private Transform playerTransform;

void Start()
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
        playerTransform = player.transform;
}

void DetectPlayer()
{
    if (playerTransform == null) return;
    // Usar playerTransform en vez de buscar cada frame
}
```

---

### 2. LayerMask invertida (confuso)

**Problema**: `obstacleMask` se niega con `~`, haciendo que el nombre sea engañoso.

**Solución A** - Renombrar:
```csharp
[SerializeField] private LayerMask layersToIgnore;  // Más claro
```

**Solución B** - No negar y seleccionar correctamente:
```csharp
[SerializeField] private LayerMask detectableLayers;  // Player, etc.
Physics.Raycast(..., detectableLayers);  // Sin ~
```

---

### 3. Cono solo en eje Y (horizontal)

**Problema**: `Quaternion.Euler(0, ±angle, 0)` solo rota en Y. No considera inclinación vertical.

**Impacto**: Si el enemigo está en una pendiente o el jugador salta, el ángulo vertical no se verifica.

**Solución** - Usar ángulo 3D real:
```csharp
// El Vector3.Angle ya es 3D, pero el cono visual no lo representa
// Para visualización correcta, usar cono 3D o ignorar si no es crítico
```

---

### 4. Sin eyePosition = fallback inconsistente

**Problema**: Si `eyePosition` es null, usa `transform.position + Vector3.up`, pero esto puede no coincidir con la altura real de los "ojos".

**Solución**: Siempre asignar eyePosition en prefab o validar en Start:
```csharp
void Start()
{
    if (eyePosition == null)
    {
        Debug.LogWarning($"[{gameObject.name}] eyePosition no asignado, usando transform");
        eyePosition = transform;
    }
}
```

---

## Optimizaciones Sugeridas

### 1. No ejecutar cada frame
```csharp
[SerializeField] private float detectionInterval = 0.1f;
private float detectionTimer;

void Update()
{
    detectionTimer += Time.deltaTime;
    if (detectionTimer >= detectionInterval)
    {
        detectionTimer = 0f;
        DetectPlayer();
    }
}
```

### 2. Usar sqrMagnitude en vez de Distance
```csharp
float sqrDistance = (playerPos - transform.position).sqrMagnitude;
if (sqrDistance > visionRange * visionRange) continue;
```

### 3. Physics.OverlapSphere primero
```csharp
// Más eficiente que FindGameObjectsWithTag
Collider[] hits = Physics.OverlapSphere(origin, visionRange, playerLayer);
foreach (Collider col in hits)
{
    // Verificar ángulo y raycast solo para los que están en rango
}
```

### 4. Cachear player en Start (ya mencionado)

---

## Relación con otros componentes

```
┌─────────────────────┐
│  EnemyVisionSensor  │
├─────────────────────┤
│ Proporciona:        │
│  ├── HasPlayerInSight
│  └── DetectedPlayer │
│                     │
│ Consumido por:      │
│  └── PatrolController
│       └── UpdateState()
│           └── Transiciones FSM
└─────────────────────┘
```

---

*Documentación para proyecto Valkor - Enero 2026*