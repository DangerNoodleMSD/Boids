# Proiect Boids

Proiectul de fata a fost creat pe ideea de Boid, introdusa de Craig Reynolds in 1986. Boids este o metoda de a crea o simulare de stoluri ce pare aleatoare si naturala prin faptul ca devine foarte diferita in functie de starea initiala a sistemului. Partea interesanta este ca acest "randomness" apare din niste reguli foarte simple. Un alt exemplu de astfel de sistem este un pendul dublu:
![alt text](Demonstrating_Chaos_with_a_Double_Pendulum.gif)

## Fișiere

### /Assets/Boids/Scripts/Boids.cs

Acest fișier conține scriptul principal pentru simularea boidelor. Scriptul gestionează inițializarea, actualizarea și mișcarea boidelor, precum și interacțiunea acestora cu pereții și limitele scenei. De asemenea, scriptul configurează și trimite datele necesare către Compute Shader pentru a calcula forțele și rotațiile boidelor.

### /Assets/Boids/Scripts/BoidCompute.compute

Acest fișier conține codul Compute Shader care este utilizat pentru a calcula forțele și rotațiile boidelor pe GPU. Compute Shader-ul procesează datele despre pozițiile, vitezele și rotațiile boidelor și aplică diverse reguli de comportament, cum ar fi separarea, alinierea și coeziunea, pentru a simula mișcarea de grup a boidelor.
