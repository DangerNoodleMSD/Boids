# Proiect Boids

## Idee generala

Proiectul de fata a fost creat pe ideea de Boid, introdusa de Craig Reynolds in 1986. Boids este o metoda de a crea o simulare de stoluri ce pare aleatoare si naturala prin faptul ca devine foarte diferita in functie de starea initiala a sistemului. Partea interesanta este ca acest "randomness" apare din niste reguli foarte simple. Un alt exemplu de astfel de sistem este un pendul dublu:

![alt text](Demonstrating_Chaos_with_a_Double_Pendulum.gif)

Dupa cum se poate vedea, parametrii initiali, ce difera foarte putin, ajung la rezultate foarte diferite.

## Principii de functionare

Pentru a intelege comportamentul boizilor este necesar sa intelegem regulile fundamentale:
1. Separation (separatie)
2. Alignment (aliniere)
3. Cohesion (coeziune)

### 1. Separation
    Separatia este folosita pentru a incerca sa facem ca boizii sa nu se izbeasca intre ei.

![alt text](Rule_separation.gif)

### 2. Alignment
    Alinierea este necesara pentru a crea stoluri in care boizii sa incerce sa mearga in aceeasi directie.

![alt text](Rule_alignment.gif)

### 3. Cohesion
    Coeziunea impinge boizii sa creeze stoluri in care fiecare dintre ei incearca sa ajunga in centru, fiind opriti de catre separatie din a se izbi unii cu altii.

![alt text](Rule_cohesion.gif)


Ideea 

## Fișiere

### /Assets/Boids/Scripts/Boids.cs

Acest fișier conține scriptul principal pentru simularea boidelor. Scriptul gestionează inițializarea, actualizarea și mișcarea boidelor, precum și interacțiunea acestora cu pereții și limitele scenei. De asemenea, scriptul configurează și trimite datele necesare către Compute Shader pentru a calcula forțele și rotațiile boidelor.

### /Assets/Boids/Scripts/BoidCompute.compute

Acest fișier conține codul Compute Shader care este utilizat pentru a calcula forțele și rotațiile boidelor pe GPU. Compute Shader-ul procesează datele despre pozițiile, vitezele și rotațiile boidelor și aplică diverse reguli de comportament, cum ar fi separarea, alinierea și coeziunea, pentru a simula mișcarea de grup a boidelor.
