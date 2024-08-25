#include "dog.h"
#include <QDebug>

Dog::Dog(string name, int age, double weight) : Pet(name, age, weight)
{
    //Set initial values for progresses
    hungerProgress = 0;
    thirstProgress = 0;
    hygieneProgress = 0;
    pottyProgress = 0;

    // Parse the lesson file
    QFile file(":/lessons/dogLessons.txt");
    file.open(QIODevice::ReadOnly);
    QTextStream in(&file);
    QString lesson = "";
    while (!in.atEnd())
    {
        QString line = in.readLine();

        if(line.contains("{foodAmount}"))
            line.replace("{foodAmount}", QString::number(Dog::calculateFood(), 'f', 1));
        if(line.contains("{waterAmount}"))
            line.replace("{waterAmount}", QString::number(calculateWater(), 'f', 1));
        if(line.contains("{sleepAmount}"))
            line.replace("{sleepAmount}", QString::number(Dog::calculateSleep(), 'f', 1));
        if (line.contains("*"))
        {
            lessons.append(lesson);
            lesson = "";
            continue;
        }
        lesson += line;
    }
    file.close();
}

double Dog::calculateFood()
{
    // Calculate food based on weight
    if (weight < 3)
        return 1.0/3;
    else if (weight < 6)
        return 1.0/2;
    else if (weight < 10)
        return 3.0/4;
    else if (weight < 15)
        return 1.0;
    else if (weight < 20)
        return 4.0/3;
    else if (weight < 30)
        return 7.0/4;
    else if (weight < 40)
        return 9.0/4;
    else if (weight < 50)
        return 8.0/3;
    else if (weight < 60)
        return 3;
    else if (weight < 70)
        return 7.0/2;
    else if (weight < 80)
        return 15.0/4;
    else if (weight < 90)
        return 17.0/4;
    else if (weight < 100)
        return 9.0/2;
    else
    {
        double additionalWeight = weight - 100;
        double additionalCups = (additionalWeight / 20) * 0.5;
        return 6 + additionalCups;
    }
}

double Dog::calculateSleep()
{
    // Calculate sleep based on age
    if (age < 1)
    {
        energyProgress = 6, maxEnergy = 6;
        return 18;
    }
    else if (age < 10)
    {
        energyProgress = 10, maxEnergy = 13;
        return 11;
    }
    else
    {
        energyProgress = 6, maxEnergy = 6;
        return 18;
    }
}

