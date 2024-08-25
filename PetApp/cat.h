#ifndef CAT_H
#define CAT_H

#include "pet.h"

/**
 * @brief The Cat class is the subclass class of pet
 */

class Cat : public Pet
{
public:
    Cat(string name, int age, double weight);  // Constructor
    virtual double calculateFood();            // Calculate food based on weight
    virtual double calculateSleep();           // Calculate sleep based on age
};

#endif // CAT_H
