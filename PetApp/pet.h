#ifndef PET_H
#define PET_H

#include <string>
#include <QVector>
#include <QDebug>
#include <QFile>
#include <QTextStream>
using std::string;

/**
 * @brief The Pet class is the base class represent a pet
 * with information and calculation of pet stats
 */
class Pet
{
protected:
    string name;    // Name of the pet
    int age;        // Pet age
    double weight;  // Pet weight

    double hungerProgress;   // The hunger progress
    double thirstProgress;   // The thirst progress
    double hygieneProgress;  // The hygiene progress
    double pottyProgress;    // The potty progress
    double energyProgress;   // The energy progress
    double maxEnergy;        // The max energy of the pet

public:
    /**
     * @brief Pet constructor
     * @param name pet name
     * @param age pet age
     * @param weight pet weight
     */
    Pet(string name, int age, double weight);
    string animal; // The animal type

    virtual double calculateFood();   // Food calculation, different for animals
    virtual double calculateSleep();  // Sleep calculation, different for animals
    double calculateWater();          // Water calcumation

    void increaseHungerProgress();    // Increase the hunger progress
    void increaseThirstProgress();    // Increase the thirst progress
    void increaseHygieneProgress();   // Increase the hygiene progress
    void increaseEnergyProgress();    // Increase the energy progress

    void decreaseHungerProgress(double);   // Decrease the hunger progress
    void decreaseThirstProgress(double);   // Decrease the thirst progress
    void decreaseEnergyProgress();         // Decrease the energy progress
    void decreaseHygieneProgress(double);  // Decrease the hygiene progress
    void decreasePottyProgress();          // Decrease the potty progress

    double getHungerPercentage();  // Get the hunger percentage
    double getThirstPercentage();  // Get the thirst percentage
    double getEnergyPercentage();  // Get the energy percentage

    double getHygieneProgress();   // Get the hygiene progress
    double getPottyProgress();     // Get the potty progress
    double getHungerProgress();    // Get the hunger progress
    double getEnergyProgress();    // Get the energy progress
    double getThirstProgress();    // Get the thirst progress

    void setHungerProgress(double);   // Set hunger progress
    void setHygieneProgress(double);  // Set hygiene progress
    void setPottyProgress(double);    // Set potty progress
    void setThirstProgress(double);   // Set thirst progress
    void setEnergyProgress(double);   // Set energy progress

    string getPetName();              // Get the pet name
    int getPetAge();                  // Get pet age
    double getPetWeight();            // Get pet weight
    string getPetType();              // Get pet type

    QVector<int> maxValues;           // The max values each activities
    QVector<QString> lessons;         // The lessons list

};

#endif // PET_H
