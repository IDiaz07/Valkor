/*using sc.terrain.proceduralpainter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using Unity.Android.Gradle;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.PlayerSettings;
using static UnityEditorInternal.ReorderableList;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float damagePerHit = 7f;
    [SerializeField] private string deathAnimationName = "Death"; // Nombre del estado en el Animator

    private float currentHealth;
    private bool isDead = false;
    private HashSet<Collider> weaponsInContact = new HashSet<Collider>();
    private Animator animator;


    [Header("Death Drop Settings")]
    [SerializeField] private GameObject deathDropPrefab;
    [SerializeField] private Vector3 dropOffset = Vector3.zero;
    [SerializeField] private bool inheritRotation = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            if (!weaponsInContact.Contains(other))
            {
                TakeDamage(damagePerHit);
                weaponsInContact.Add(other);
                Debug.Log($"Golpeado por {other.gameObject.name}. Vida: {currentHealth}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            if (!weaponsInContact.Contains(other))
            {
                TakeDamage(damagePerHit);
                weaponsInContact.Add(other);
                Debug.Log($"Golpeado por {other.gameObject.name}. Vida: {currentHealth}");
            }
        }
    }

    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} ha muerto");

        DisableEnemyComponents();

        // Reproduce la animación
        if (animator != null)
        {
            animator.SetTrigger("die");
        }

        DisableEnemyComponents();

        InstantiateDeathDrop();

        // Espera a que termine la animación
        StartCoroutine(WaitForDeathAnimation());
    }

    private void InstantiateDeathDrop()
    {
        // Verificar si hay un prefab asignado
        if (deathDropPrefab == null)
        {
            Debug.Log("No hay prefab de muerte asignado");
            return;
        }

        // Calcular la posición de spawn
        Vector3 spawnPosition = transform.position + dropOffset;

        // Calcular la rotación de spawn
        Quaternion spawnRotation = inheritRotation ? transform.rotation : Quaternion.identity;

        // Instanciar el prefab
        GameObject droppedObject = Instantiate(deathDropPrefab, spawnPosition, spawnRotation);

        Debug.Log($"Objeto instanciado en muerte: {droppedObject.name} en posición {spawnPosition}");
    }



    private void DisableEnemyComponents()
    {
        // Desactiva el PatrolController primero (esto es crítico)
        PatrolController patrol = GetComponent<PatrolController>();
        if (patrol != null)
        {
            patrol.enabled = false;
            Debug.Log("PatrolController desactivado");
        }

        // NavMeshAgent - CRÍTICO: verificar isOnNavMesh PRIMERO
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            // Solo manipula el agente si está en el NavMesh
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                Debug.Log("✓ NavMeshAgent detenido");
            }

            // Siempre desactívalo al final
            agent.enabled = false;
            Debug.Log("✓ NavMeshAgent desactivado");
        }

        // Desactiva el collider para no recibir más golpes
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("Collider desactivado");
        }

        // Si tienes EnemyVisionSensor, desactívalo también
        EnemyVisionSensor visionSensor = GetComponent<EnemyVisionSensor>();
        if (visionSensor != null)
        {
            visionSensor.enabled = false;
            Debug.Log("VisionSensor desactivado");
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Espera un frame para asegurarse de que la animación comenzó
        yield return null;

        // Espera hasta que el Animator esté en el estado de muerte
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationName))
        {
            yield return null;
        }

        // Ahora espera hasta que la animación termine
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        Debug.Log("Animación de muerte completada");

        // AHORA sí destruye el objeto
        Destroy(this.gameObject);
    }
}






# EnemyHealth - Documentación Técnica

## Índice
1. [Descripción General](#descripción-general)
2. [Arquitectura](#arquitectura)
3. [Configuración en Inspector](#configuración-en-inspector)
4. [Métodos](#métodos)
5. [Sistema de Detección de Daño](#sistema-de-detección-de-daño)
6. [Sistema de Muerte](#sistema-de-muerte)
7. [Dependencias](#dependencias)
8. [Problemas Conocidos y Soluciones](#problemas-conocidos-y-soluciones)
9. [Debugging](#debugging)
    
---

## Descripción General

`EnemyHealth` gestiona la vida, daño y muerte de los enemigos.Implementa detección de golpes mediante triggers, prevención de daño múltiple con HashSet, y una secuencia de muerte con animación, drop de items y destrucción.

### Responsabilidades:
- Gestionar vida actual del enemigo
- Detectar colisiones con armas(layer "Weapon")
- Prevenir múltiples registros de daño por el mismo golpe
- Ejecutar secuencia de muerte(animación → drop → destrucción)
- Desactivar componentes de IA al morir

-- -

## Arquitectura

### Flujo de daño
```
Weapon Collider → OnTriggerEnter() → ¿isDead ? → ¿Layer Weapon ? → ¿Ya en HashSet ?
                                         │            │                │
                                        [SKIP][SKIP]     NO → TakeDamage() → ¿HP≤0 ? → Die()
                                                                SÍ → [SKIP]
```

### Secuencia de muerte
```
Die() → isDead = true → DisableComponents() → Trigger "die" → InstantiateDrop() → WaitForAnimation() → Destroy()
```

---

## Configuración en Inspector

### Variables de Vida

| Campo | Tipo | Default | Descripción |
| -------| ------| ---------| -------------|
| `maxHealth` | float | 25f | Vida máxima |
| `damagePerHit` | float | 7f | Daño por golpe |
| `deathAnimationName` | string | "Death" | Nombre del estado de muerte en Animator |

### Death Drop Settings

| Campo | Tipo | Default | Descripción |
| -------| ------| ---------| -------------|
| `deathDropPrefab` | GameObject | null | Prefab a instanciar al morir |
| `dropOffset` | Vector3 | (0, 0, 0) | Offset de posición del spawn |
| `inheritRotation` | bool | false | Si hereda rotación del enemigo |

### Variables Runtime (privadas)

| Variable | Tipo | Descripción |
| ----------| ------| -------------|
| `currentHealth` | float | Vida actual |
| `isDead` | bool | Previene múltiples muertes |
| `weaponsInContact` | HashSet\< Collider\> | Armas que ya golpearon |
| `animator` | Animator | Referencia cacheada |

---

## Métodos

### Ciclo de Vida Unity

| Método | Descripción |
| --------| -------------|
| `Start()` | Inicializa `currentHealth = maxHealth`, cachea Animator |
| `OnTriggerEnter()` | Detecta golpes, verifica layer y HashSet, aplica daño |
| `OnTriggerExit()` | ⚠️ **BUG * *: Lógica incorrecta(ver problemas conocidos) |

### Sistema de Daño

| Método | Descripción |
| --------| -------------|
| `TakeDamage(float)` | Reduce vida, llama `Die()` si HP ≤ 0 |
| `Die()` | Secuencia completa de muerte |
| `DisableEnemyComponents()` | Desactiva PatrolController, NavMeshAgent, Collider, VisionSensor |
| `InstantiateDeathDrop()` | Instancia prefab de loot |
| `WaitForDeathAnimation()` | Corrutina que espera animación y destruye GO |

### Orden de desactivación en muerte

```
1.PatrolController.enabled = false  ← Primero(evita navegación)
2.NavMeshAgent.isStopped = true     ← Verificando isOnNavMesh
3.NavMeshAgent.enabled = false
4.Collider.enabled = false          ← Evita más daño
5.EnemyVisionSensor.enabled = false
```

---

## Sistema de Detección de Daño

### Problema del "doble golpe"
Un arma puede registrar múltiples colisiones durante un swing.Solución: **HashSet * *.

```csharp
private HashSet<Collider> weaponsInContact = new HashSet<Collider>();

// En OnTriggerEnter:
if (!weaponsInContact.Contains(other))
{
    TakeDamage(damagePerHit);
    weaponsInContact.Add(other);
}
```

**⚠️ Problema actual**: El HashSet nunca se limpia (ver bugs).

---

## Sistema de Muerte

### Timeline
```
t = 0.000s Die() → isDead = true, desactivar componentes, trigger "die", spawn drop
t=0.016s  yield return null (esperar 1 frame)
t = ~0.1s Animator entra en estado "Death"
t=~2.0s   normalizedTime >= 1.0 → Destroy(gameObject)
```

### Espera de animación
```csharp
// Esperar que entre en estado de muerte
while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationName))
    yield return null;

// Esperar que termine (normalizedTime = 0→1)
while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
    yield return null;

Destroy(gameObject);
```

---

## Dependencias

### Componentes requeridos

| Componente | Obligatorio | Notas |
| ------------| -------------| -------|
| Collider(Trigger) | ✅ | Para detectar golpes |
| Animator | ⚠️ Recomendado | Para animación de muerte |

### Componentes desactivados al morir

- PatrolController
- NavMeshAgent  
- Collider
- EnemyVisionSensor

### Configuración de proyecto

- Layer **"Weapon"** debe existir
- Trigger **"die"** en Animator Controller
- Estado de animación con nombre exacto de `deathAnimationName`

---

## Problemas Conocidos y Soluciones

### 1. ⚠️ CRÍTICO: OnTriggerExit incorrecto

**Problema**: Copia exacta de OnTriggerEnter, no limpia el HashSet.

```csharp
// ACTUAL (incorrecto) - aplica daño al salir
if (!weaponsInContact.Contains(other))
    TakeDamage(damagePerHit);

// CORRECTO - limpiar HashSet
weaponsInContact.Remove(other);
```

**Impacto * *: El arma solo puede golpear una vez por vida del enemigo.

---

### 2. DisableEnemyComponents() llamado dos veces

**Problema**: En `Die()`, líneas 72 y 79 llaman al mismo método.

**Solución**: Eliminar la segunda llamada.

---

### 3. Sin validación de Animator

**Problema**: `WaitForDeathAnimation()` falla si no hay Animator.

**Solución**:
```csharp
if (animator == null)
{
    Destroy(gameObject);
    yield break;
}
```

---

### 4. Corrutinas de PatrolController no se detienen

**Solución * *: Añadir en `DisableEnemyComponents()`:
```csharp
patrol.StopAllCoroutines();
patrol.enabled = false;
```

---

## Debugging

### Mensajes de Log

| Mensaje | Momento |
| ---------| ---------|
| `"Golpeado por {name}. Vida: {hp}"` | OnTriggerEnter exitoso |
| `"Vida restante: {current}/{max}"` | TakeDamage() |
| `"{name} ha muerto"` | Die() |
| `"✓ NavMeshAgent detenido/desactivado"` | DisableComponents() |
| `"Objeto instanciado en muerte: {name}"` | InstantiateDeathDrop() |
| `"Animación de muerte completada"` | Antes de Destroy |

### Verificación rápida
1. Golpear enemigo → vida decrementa por `damagePerHit`
2. Mismo swing → no registra daño múltiple
3. Al morir → componentes desactivados en Inspector
4. Drop aparece → posición correcta con offset
5. Animación termina → GameObject destruido

---

*Documentación para proyecto Valkor - Enero 2026*# EnemyHealth - Documentación Técnica

## Índice
1. [Descripción General](#descripción-general)
2. [Arquitectura](#arquitectura)
3. [Configuración en Inspector](#configuración-en-inspector)
4. [Métodos](#métodos)
5. [Sistema de Detección de Daño](#sistema-de-detección-de-daño)
6. [Sistema de Muerte](#sistema-de-muerte)
7. [Dependencias](#dependencias)
8. [Problemas Conocidos y Soluciones](#problemas-conocidos-y-soluciones)
9. [Debugging](#debugging)

---

## Descripción General

`EnemyHealth` gestiona la vida, daño y muerte de los enemigos.Implementa detección de golpes mediante triggers, prevención de daño múltiple con HashSet, y una secuencia de muerte con animación, drop de items y destrucción.

### Responsabilidades:
- Gestionar vida actual del enemigo
- Detectar colisiones con armas(layer "Weapon")
- Prevenir múltiples registros de daño por el mismo golpe
- Ejecutar secuencia de muerte(animación → drop → destrucción)
- Desactivar componentes de IA al morir

-- -

## Arquitectura

### Flujo de daño
```
Weapon Collider → OnTriggerEnter() → ¿isDead ? → ¿Layer Weapon ? → ¿Ya en HashSet ?
                                         │            │                │
                                        [SKIP][SKIP]     NO → TakeDamage() → ¿HP≤0 ? → Die()
                                                                SÍ → [SKIP]
```

### Secuencia de muerte
```
Die() → isDead = true → DisableComponents() → Trigger "die" → InstantiateDrop() → WaitForAnimation() → Destroy()
```

---

## Configuración en Inspector

### Variables de Vida

| Campo | Tipo | Default | Descripción |
| -------| ------| ---------| -------------|
| `maxHealth` | float | 25f | Vida máxima |
| `damagePerHit` | float | 7f | Daño por golpe |
| `deathAnimationName` | string | "Death" | Nombre del estado de muerte en Animator |

### Death Drop Settings

| Campo | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `deathDropPrefab` | GameObject | null | Prefab a instanciar al morir |
| `dropOffset` | Vector3 | (0,0,0) | Offset de posición del spawn |
| `inheritRotation` | bool | false | Si hereda rotación del enemigo |

### Variables Runtime (privadas)

| Variable | Tipo | Descripción |
|----------|------|-------------|
| `currentHealth` | float | Vida actual |
| `isDead` | bool | Previene múltiples muertes |
| `weaponsInContact` | HashSet\<Collider\> | Armas que ya golpearon |
| `animator` | Animator | Referencia cacheada |

---

## Métodos

### Ciclo de Vida Unity

| Método | Descripción |
|--------|-------------|
| `Start()` | Inicializa `currentHealth = maxHealth`, cachea Animator |
| `OnTriggerEnter()` | Detecta golpes, verifica layer y HashSet, aplica daño |
| `OnTriggerExit()` | ⚠️ **BUG**: Lógica incorrecta (ver problemas conocidos) |

### Sistema de Daño

| Método | Descripción |
|--------|-------------|
| `TakeDamage(float)` | Reduce vida, llama `Die()` si HP ≤ 0 |
| `Die()` | Secuencia completa de muerte |
| `DisableEnemyComponents()` | Desactiva PatrolController, NavMeshAgent, Collider, VisionSensor |
| `InstantiateDeathDrop()` | Instancia prefab de loot |
| `WaitForDeathAnimation()` | Corrutina que espera animación y destruye GO |

### Orden de desactivación en muerte

```
1. PatrolController.enabled = false  ← Primero (evita navegación)
2. NavMeshAgent.isStopped = true     ← Verificando isOnNavMesh
3. NavMeshAgent.enabled = false
4. Collider.enabled = false          ← Evita más daño
5. EnemyVisionSensor.enabled = false
```

---

## Sistema de Detección de Daño

### Problema del "doble golpe"
Un arma puede registrar múltiples colisiones durante un swing. Solución: **HashSet**.

```csharp
private HashSet<Collider> weaponsInContact = new HashSet<Collider>();

// En OnTriggerEnter:
if (!weaponsInContact.Contains(other))
{
    TakeDamage(damagePerHit);
    weaponsInContact.Add(other);
}
```

**⚠️ Problema actual**: El HashSet nunca se limpia (ver bugs).

---

## Sistema de Muerte

### Timeline
```
t=0.000s  Die() → isDead=true, desactivar componentes, trigger "die", spawn drop
t=0.016s  yield return null (esperar 1 frame)
t=~0.1s   Animator entra en estado "Death"
t=~2.0s   normalizedTime >= 1.0 → Destroy(gameObject)
```

### Espera de animación
```csharp
// Esperar que entre en estado de muerte
while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationName))
    yield return null;

// Esperar que termine (normalizedTime = 0→1)
while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
    yield return null;

Destroy(gameObject);
```

---

## Dependencias

### Componentes requeridos

| Componente | Obligatorio | Notas |
|------------|-------------|-------|
| Collider (Trigger) | ✅ | Para detectar golpes |
| Animator | ⚠️ Recomendado | Para animación de muerte |

### Componentes desactivados al morir

- PatrolController
- NavMeshAgent  
- Collider
- EnemyVisionSensor

### Configuración de proyecto

- Layer **"Weapon"** debe existir
- Trigger **"die"** en Animator Controller
- Estado de animación con nombre exacto de `deathAnimationName`

---

## Debugging

### Mensajes de Log

| Mensaje | Momento |
|---------|---------|
| `"Golpeado por {name}. Vida: {hp}"` | OnTriggerEnter exitoso |
| `"Vida restante: {current}/{max}"` | TakeDamage() |
| `"{name} ha muerto"` | Die() |
| `"✓ NavMeshAgent detenido/desactivado"` | DisableComponents() |
| `"Objeto instanciado en muerte: {name}"` | InstantiateDeathDrop() |
| `"Animación de muerte completada"` | Antes de Destroy |

### Verificación rápida
1. Golpear enemigo → vida decrementa por `damagePerHit`
2. Mismo swing → no registra daño múltiple
3. Al morir → componentes desactivados en Inspector
4. Drop aparece → posición correcta con offset
5. Animación termina → GameObject destruido

---

*Documentación para proyecto Valkor - Enero 2026*

*/