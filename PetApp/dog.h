#ifndef DOG_H
#define DOG_H

#include "pet.h"

/**
 * @brief The Dog class is the subclass class of pet
 */

class Dog : public Pet
{
public:
    Dog(string name, int age, double weight);  // Constructor
    virtual double calculateFood();            // Calculate food based on weight
    virtual double calculateSleep();           // Calculate sleep based on age

};

#endif // DOG_H
