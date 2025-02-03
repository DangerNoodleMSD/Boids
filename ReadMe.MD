# Proiect Boids

## Idee generala

Proiectul de fata a fost creat pe ideea de Boid, introdusa de Craig Reynolds in 1986. Boids este o metoda de a crea o simulare de stoluri ce pare aleatoare si naturala prin faptul ca devine foarte diferita in functie de starea initiala a sistemului. Partea interesanta este ca acest "randomness" apare din niste reguli foarte simple. Un alt exemplu de astfel de sistem este un pendul dublu:
<div style="text-align: center">
    <img src="Demonstrating_Chaos_with_a_Double_Pendulum.gif" style="width:50%"></img>
</div>

Dupa cum se poate vedea, parametrii initiali, ce difera foarte putin, ajung la rezultate foarte diferite.

## Principii de functionare

Pentru a intelege comportamentul boizilor este necesar sa intelegem regulile fundamentale:
**1. Separation (separatie)**
**2. Alignment (aliniere)**
**3. Cohesion (coeziune)**

### 1. Separation

Separatia este folosita pentru a incerca sa facem ca boizii sa nu se izbeasca intre ei.

<div style="text-align: center">
    <img src="Rule_separation.gif" style="width:50%"></img>
</div>

### 2. Alignment

Alinierea este necesara pentru a crea stoluri in care boizii sa incerce sa mearga in aceeasi directie.

<div style="text-align: center">
    <img src="Rule_alignment.gif" style="width:50%"></img>
</div>

### 3. Cohesion

Coeziunea impinge boizii sa creeze stoluri in care fiecare dintre ei incearca sa ajunga in centru, fiind opriti de catre separatie din a se izbi unii cu altii.

<div style="text-align: center">
    <img src="Rule_cohesion.gif" style="width:50%"></img>
</div>

## Implementare

Problema care am incercat sa o rezolv este a vitezei de calcul a fortelor dintre boizi (trebuie ca fiecare boid sa verifice pozitiile tuturor celorlalti boizi ceea ce face ca programul sa aiba o complexitate de O(n<sup>2</sup>) in relatie cu numarul de boizi). Prima solutie evidenta este sa facem calculele secvential pe CPU. Aceasta abordare devine ineficienta foarte rapid cu numar marit de boizi. Solutia acestei probleme folosita de mine a fost utilizarea puterii de calcul in paralel al GPU-ului (CPU-ul meu I7-11700kf are 8 core-uri, pe cand GPU-ul RTX 2070 Super are 2560). O alta solutie ce nu va fi discutata aici si ce nu presupune brute force este un Quad Tree, dar aceasta este o implementare pe CPU.

## Fișiere

### /Assets/Boids/Scripts/Boids.cs

Fisierul Boids.cs ruleaza pe CPU. El este necesar deoarece GPU-ul nu poate interactiona cu Game Object-uri (boizii) si functiile game engine-ului in general. Setarile importante comportamentului boizilor sunt localizate in struct-ul BoidBehaviour ce este creat ca un camp serializabil (este important pentru a putea schimba valorile variabilelor din unity si nu din cod in sine). Majoritatea codului de aici se axeaza pe setarea mediului atunci cand valori despre boizi (numarul lor, comportamentul) si interactiunea lor cu peretii. Partea importanta se intampla in **DispatchComputeShader** si **MoveBoids**.

**DispatchComputeShader**
Aici sunt incarcate pozitiile boizilor, vitezele lor si un vector de float-uri random (este mai greu sa creezi randomness pe GPU decat pe CPU). Apoi este pornit compute shader-ul de pe GPU si sunt luate inapoi fortele ce trebuie exercitate.

**MoveBoids**
In aceasta functie este preluat vectorul de forte si sunt aplicate pe fiecare boid.

### /Assets/Boids/Scripts/BoidCompute.compute

In acest fisier se calculeaza fortele boizilor. Fiecare instanta de functie ce ruleaza foloseste variabila **id.x** pentru a cunoaste pe care boid trebuie sa lucreze.
