#include "pet.h"
#include <QDebug>

Pet::Pet(string name, int age, double weight) : name(name), age(age), weight(weight)
{
    // Set initial state of pet
    hungerProgress = 0;
    thirstProgress = 0;
    hygieneProgress = 0;
    pottyProgress = 0;
    energyProgress = 100;
}

double Pet::calculateFood() // Will be done by subclasses
{
    return 0;
}

double Pet::calculateSleep() // Will be done by subclasses
{
    return 0;
}

double Pet::calculateWater()
{
    return weight / 6.0;
}

void Pet::increaseHungerProgress()
{
    if (hungerProgress + 0.0005 >= calculateFood()) // If the progress > max, set to max
        hungerProgress = calculateFood();
    else
        hungerProgress = hungerProgress + 0.0005;
}

void Pet::increaseThirstProgress()
{
    if (thirstProgress + 0.004 >= calculateWater()) // If the progress > max, set to max
        thirstProgress = calculateWater();
    else
        thirstProgress = thirstProgress + 0.004;
}

void Pet::increaseHygieneProgress()
{
    hygieneProgress++;
}

void Pet::decreaseHungerProgress(double amount)
{
    if (hungerProgress > 0) // If the progress > 0, decrease
        hungerProgress = hungerProgress - amount * calculateFood();
}

void Pet::decreaseThirstProgress(double amount)
{
    if (thirstProgress > 0) // If the progress > 0, decrease
        thirstProgress = thirstProgress - amount * calculateWater();
}

void Pet::increaseEnergyProgress()
{
    if (energyProgress + (0.166666 * ((maxEnergy) / (24 - maxEnergy))) > maxEnergy)
        energyProgress = maxEnergy;
    else
        energyProgress = energyProgress + (0.166666 * ((maxEnergy) / (24 - maxEnergy)));
}

void Pet::decreaseEnergyProgress()
{
    if (energyProgress - 0.166666 < 0)
        energyProgress = 0;
    else
        energyProgress = energyProgress - 0.166666;
}

void Pet::decreaseHygieneProgress(double amount)
{
    if (hygieneProgress - amount < 0)
        hygieneProgress = 0;
    else if (hygieneProgress > 0 || hygieneProgress == 0)
        hygieneProgress -= amount;
}

double Pet::getHungerPercentage()
{
    if (hungerProgress / calculateFood() > 1.0)
        return 1;
    return hungerProgress / calculateFood();
}

double Pet::getThirstPercentage()
{
    if (thirstProgress / calculateWater() > 1.0)
        return 1;
    return thirstProgress / calculateWater();
}

double Pet::getEnergyPercentage()
{
    return energyProgress / maxEnergy;
}

double Pet::getHygieneProgress()
{
    return hygieneProgress;
}

double Pet::getPottyProgress()
{
    return pottyProgress;
}

double Pet::getHungerProgress()
{
    return hungerProgress;
}

double Pet::getThirstProgress()
{
    return thirstProgress;
}

double Pet::getEnergyProgress()
{
    return energyProgress;
}

string Pet::getPetName()
{
    return name;
}

int Pet::getPetAge()
{
    return age;
}

double Pet::getPetWeight()
{
    return weight;
}

string Pet::getPetType()
{
    return animal;
}

void Pet::setHungerProgress(double progress)
{
    hungerProgress = progress;
}

void Pet::setHygieneProgress(double progress)
{
    hygieneProgress = progress;
}

void Pet::setPottyProgress(double progress)
{
    pottyProgress = progress;
}

void Pet::setThirstProgress(double progress)
{
    thirstProgress = progress;
}

void Pet::setEnergyProgress(double progress)
{
    energyProgress = progress;
}

void Pet::decreasePottyProgress()
{
    pottyProgress -= 1;
}



