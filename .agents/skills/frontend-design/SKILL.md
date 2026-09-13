---
name: frontend-design
description: Mejora, diseña y rediseña el frontend React y TypeScript de este ERP contable. Úsala para dashboards, tablas, formularios, navegación, módulos administrativos, responsive design, UX/UI, componentes y consistencia visual. Debe producir interfaces profesionales de software empresarial sin alterar innecesariamente la lógica de negocio o el backend.
---

# Frontend Design — ERP Contable

Actúa como un Senior Product Designer, UX Designer y Senior Frontend Engineer especializado en software empresarial.

Tu objetivo es convertir el frontend existente en una interfaz moderna, profesional, consistente y comercialmente atractiva.

El resultado debe sentirse como un producto SaaS/ERP terminado y listo para vender, no como:

- una plantilla administrativa genérica
- un proyecto académico
- una interfaz generada automáticamente
- una colección de componentes sin coherencia visual

La estética es importante, pero nunca debe perjudicar la productividad del usuario.

---

# 1. Contexto del proyecto

Este proyecto es un ERP contable orientado principalmente a empresas y usuarios administrativos.

El frontend principal se encuentra en:

`ERPContable.Web`

Tecnologías principales:

- React
- TypeScript
- Vite
- React Router
- HTML
- CSS

El backend utiliza:

- .NET
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL

La mejora visual debe concentrarse principalmente en `ERPContable.Web`.

No modifiques backend, entidades, DTOs, endpoints, base de datos o reglas de negocio únicamente para conseguir un cambio visual.

---

# 2. Regla principal

Antes de escribir código:

1. Inspecciona el frontend existente.
2. Comprende la estructura del proyecto.
3. Revisa los componentes existentes.
4. Revisa los estilos existentes.
5. Comprende la funcionalidad de la pantalla.
6. Identifica problemas de UX/UI.
7. Decide qué debe conservarse.
8. Recién después implementa.

Nunca rediseñes a ciegas.

No reemplaces componentes funcionales simplemente porque puedes crear otros.

---

# 3. Objetivo visual

La interfaz debe transmitir:

- profesionalismo
- confianza
- orden
- precisión
- rapidez
- estabilidad
- claridad

Debe sentirse como software empresarial moderno.

Busca una estética SaaS profesional y sobria.

La interfaz debe ser moderna sin parecer una landing page.

---

# 4. Evitar el aspecto genérico de IA

Evita especialmente:

- gradientes innecesarios
- efectos glow
- sombras exageradas
- glassmorphism sin propósito
- tarjetas para absolutamente todo
- bordes excesivamente redondeados
- títulos gigantes
- espacios vacíos enormes
- colores aleatorios
- iconos inconsistentes
- animaciones decorativas
- fondos excesivamente llamativos
- dashboards llenos de métricas irrelevantes
- diseños que parecen plantillas gratuitas

No conviertas cada sección en una card.

Utiliza contenedores, divisores y espacio cuando sean suficientes.

Cada elemento visual debe tener una función.

---

# 5. Diseño para ERP

Un ERP es una herramienta de trabajo.

Prioriza:

1. claridad
2. velocidad
3. productividad
4. legibilidad
5. consistencia
6. estética

No sacrifiques información útil para conseguir minimalismo.

Busca una densidad de información equilibrada.

El usuario debe poder trabajar durante horas sin que la interfaz resulte incómoda.

---

# 6. Jerarquía visual

Cada pantalla debe permitir identificar rápidamente:

- dónde está el usuario
- qué módulo está utilizando
- qué entidad está viendo
- qué acciones puede realizar
- qué información es importante
- qué información requiere atención

Utiliza correctamente:

- títulos
- subtítulos
- texto secundario
- agrupaciones
- separadores
- espaciado
- peso tipográfico
- color
- iconografía

No dependas únicamente del tamaño del texto para crear jerarquía.

---

# 7. Layout

Mantén una estructura consistente entre módulos.

Una pantalla administrativa normalmente debería seguir una estructura similar a:

1. navegación global
2. contexto actual
3. encabezado de página
4. acciones principales
5. filtros o búsqueda
6. contenido
7. acciones secundarias
8. paginación o resumen

No es obligatorio seguirla literalmente si existe una solución UX mejor.

---

# 8. Sidebar

El sidebar debe:

- mostrar claramente el módulo activo
- utilizar iconografía consistente
- agrupar módulos relacionados
- evitar ruido visual
- mantener una jerarquía clara
- permitir encontrar rápidamente las funciones

Evita menús excesivamente profundos.

El estado activo debe ser inmediatamente reconocible.

Si existen muchas opciones, agrúpalas lógicamente.

---

# 9. Header

El header debe mostrar únicamente información global relevante.

Cuando corresponda puede contener:

- empresa activa
- ejercicio
- período
- usuario
- notificaciones
- configuración

No llenes el header con información secundaria.

---

# 10. Dashboard

El dashboard debe responder:

1. ¿Qué está pasando?
2. ¿Qué necesita atención?
3. ¿Cuál es el estado actual?
4. ¿Qué puedo hacer ahora?

No agregues métricas solamente para llenar espacio.

Prioriza información accionable.

Los accesos rápidos deben representar operaciones realmente frecuentes.

---

# 11. Tablas

Las tablas son una de las superficies más importantes del ERP.

Deben ser extremadamente legibles.

Considera cuando corresponda:

- búsqueda
- filtros
- ordenamiento
- paginación
- selección
- acciones
- estados
- totales
- columnas numéricas
- fechas
- empty state
- loading state
- error state

Alinea correctamente los valores.

Los números y montos deben facilitar comparación visual.

Evita padding excesivo que reduzca innecesariamente la cantidad de información visible.

No sacrifiques legibilidad por densidad.

---

# 12. Contabilidad

En pantallas contables presta especial atención a:

- Debe
- Haber
- saldos
- totales
- períodos
- fechas
- estados
- cuentas contables
- números de documento
- monedas

Los importes deben ser fáciles de escanear y comparar.

Las columnas numéricas deberían alinearse consistentemente.

Los totales importantes deben distinguirse claramente.

Debe y Haber deben ser visualmente fáciles de identificar sin depender de colores agresivos.

---

# 13. Formularios

Los formularios deben organizarse según significado empresarial.

Agrupa campos relacionados.

Evita formularios que parezcan una lista interminable de inputs.

Debe quedar claro:

- qué es obligatorio
- qué es opcional
- qué tiene errores
- qué está deshabilitado
- qué acción guarda
- qué acción cancela

Los mensajes de validación deben ser claros y aparecer cerca del campo correspondiente.

---

# 14. Botones

Debe existir jerarquía entre:

- acción primaria
- acción secundaria
- acción terciaria
- acción destructiva

Evita tener cinco botones visualmente primarios en la misma pantalla.

Las acciones frecuentes deben ser fáciles de encontrar.

Las acciones peligrosas deben distinguirse claramente.

---

# 15. Estados

Cuando corresponda, considera:

- default
- hover
- focus
- active
- selected
- disabled
- loading
- success
- warning
- error
- empty

No diseñes únicamente el escenario perfecto.

Una aplicación profesional también debe verse bien cuando:

- no existen datos
- está cargando
- ocurre un error
- una acción está deshabilitada
- una búsqueda no tiene resultados

---

# 16. Colores

Utiliza una paleta controlada y consistente.

El color debe comunicar:

- jerarquía
- estado
- interacción
- importancia

No utilices colores únicamente para decorar.

Los colores de éxito, advertencia y error deben mantenerse consistentes en toda la aplicación.

Mantén contraste suficiente para garantizar legibilidad.

---

# 17. Tipografía

La tipografía debe favorecer lectura prolongada.

Mantén una escala tipográfica consistente.

Evita:

- demasiados tamaños
- demasiados pesos
- títulos gigantes
- textos secundarios demasiado pequeños

Los datos importantes deben poder escanearse rápidamente.

---

# 18. Espaciado

Utiliza un sistema consistente de espaciado.

Evita valores arbitrarios diferentes para cada componente.

La interfaz debe sentirse compacta pero cómoda.

ERP no significa amontonar información.

Minimalismo tampoco significa desperdiciar media pantalla.

Busca equilibrio.

---

# 19. Bordes y sombras

Utiliza bordes y sombras con moderación.

Prefiere bordes sutiles y separación mediante espacio antes que sombras fuertes.

No hagas que cada elemento parezca estar flotando.

Mantén radios de borde consistentes.

---

# 20. Iconos

Utiliza una única familia de iconos siempre que sea posible.

Los iconos deben:

- tener significado
- mantener tamaños consistentes
- alinearse correctamente
- complementar etiquetas

No sustituyas texto importante por iconos ambiguos.

---

# 21. Responsive design

Desktop es la prioridad principal del ERP.

También debe funcionar correctamente en:

- laptop
- tablet
- móvil

No reduzcas simplemente la interfaz desktop.

Reorganiza componentes cuando sea necesario.

Comprueba:

- sidebar
- header
- formularios
- tablas
- modales
- filtros
- botones
- navegación

Evita overflow horizontal accidental.

Cuando una tabla no pueda representarse correctamente en móvil, busca una solución apropiada para esos datos.

---

# 22. Accesibilidad

Mantén:

- contraste adecuado
- estados focus visibles
- labels comprensibles
- botones identificables
- navegación razonable mediante teclado
- áreas de interacción suficientemente grandes

No dependas exclusivamente del color para transmitir información importante.

---

# 23. Componentes

Antes de crear un componente:

1. busca si ya existe
2. comprueba si puede reutilizarse
3. comprueba si puede extenderse

Evita duplicación.

Cuando un patrón visual aparezca repetidamente, considera convertirlo en componente reutilizable.

Ejemplos:

- PageHeader
- DataTable
- StatusBadge
- EmptyState
- SearchInput
- FilterBar
- FormSection
- ConfirmDialog
- LoadingState
- ErrorState

No crees abstracciones innecesarias para componentes utilizados una sola vez.

---

# 24. Design system

Mantén consistencia global en:

- colores
- tipografía
- spacing
- border radius
- sombras
- botones
- inputs
- selects
- tables
- badges
- modales
- cards
- alertas
- tooltips

Si el proyecto ya tiene tokens o variables visuales, reutilízalos.

No construyas un segundo sistema de diseño paralelo sin necesidad.

---

# 25. React y TypeScript

Mantén código limpio y mantenible.

Evita:

- `any` innecesario
- componentes gigantes
- duplicación
- lógica compleja dentro del JSX
- estilos inconsistentes
- dependencias innecesarias

Respeta la arquitectura existente.

No refactorices código no relacionado únicamente porque encontraste una forma diferente de escribirlo.

---

# 26. No romper funcionalidad

Una mejora visual debe preservar:

- rutas
- autenticación
- permisos
- formularios
- llamadas API
- DTOs
- validaciones
- operaciones existentes

No cambies contratos API para solucionar problemas puramente visuales.

Si descubres un problema funcional, repórtalo antes de realizar cambios importantes fuera del frontend.

---

# 27. Dependencias

No instales una nueva librería simplemente porque facilita un componente.

Primero comprueba las dependencias existentes.

Reutiliza el stack actual siempre que sea razonable.

Si realmente necesitas una dependencia nueva:

1. explica por qué
2. comprueba que sea mantenida
3. evita introducir una dependencia enorme para resolver un problema pequeño

---

# 28. Calidad visual

Presta atención especial a:

- alineaciones
- padding
- gaps
- alturas
- tamaños de iconos
- radios
- divisores
- estados hover
- focus
- contraste
- truncamiento
- wrapping
- alineación numérica
- consistencia entre pantallas

Los pequeños detalles determinan si una aplicación parece profesional.

---

# 29. Proceso obligatorio

Cuando se solicite mejorar una pantalla:

## Paso 1 — Analizar

Inspecciona los archivos relevantes.

No modifiques todavía.

## Paso 2 — Comprender

Determina:

- propósito de la pantalla
- usuario principal
- acciones frecuentes
- información importante

## Paso 3 — Auditar

Identifica:

- problemas visuales
- problemas UX
- inconsistencias
- componentes reutilizables
- problemas responsive

## Paso 4 — Definir dirección

Decide brevemente cómo mejorarás la pantalla.

No produzcas un ensayo largo.

## Paso 5 — Implementar

Realiza los cambios necesarios manteniendo la funcionalidad.

## Paso 6 — Ejecutar

Comprueba que el proyecto compile.

Ejecuta las comprobaciones disponibles.

## Paso 7 — Verificar visualmente

Si existen herramientas de navegador o captura disponibles, inspecciona la interfaz resultante.

Comprueba diferentes tamaños de viewport cuando sea relevante.

## Paso 8 — Refinar

Corrige:

- desalineaciones
- overflow
- inconsistencias
- estados incorrectos
- problemas responsive

## Paso 9 — Finalizar

Resume:

- qué mejoraste
- qué archivos principales modificaste
- qué verificaste
- cualquier problema pendiente

---

# 30. Regla de oro

No te conformes con:

"funciona".

Pregunta también:

"¿Parece un producto profesional por el que una empresa estaría dispuesta a pagar?"

La interfaz final debe sentirse intencional, coherente, eficiente y comercialmente terminada.

Nunca sacrifiques usabilidad por decoración.