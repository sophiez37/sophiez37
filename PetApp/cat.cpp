#include "cat.h"

Cat::Cat(string name, int age, double weight) : Pet(name, age, weight)
{
    // Setup initial progresses
    hungerProgress = 0;
    thirstProgress = 0;
    hygieneProgress = 0;
    pottyProgress = 0;

    // Parse the lesson file
    QFile file(":/lessons/catLessons.txt");
    file.open(QIODevice::ReadOnly);
    QTextStream in(&file);
    QString lesson = "";
    while (!in.atEnd())
    {
        QString line = in.readLine();

        if (line.contains("{foodAmount}"))
            line.replace("{foodAmount}", QString::number(Cat::calculateFood(), 'f', 1));
        if (line.contains("{waterAmount}"))
            line.replace("{waterAmount}", QString::number(calculateWater(), 'f', 1));
        if (line.contains("{sleepAmount}"))
            line.replace("{sleepAmount}", QString::number(Cat::calculateSleep(), 'f', 1));
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

double Cat::calculateFood()
{
    // Calculate the food based on weight
    if (weight < 4)
        return 1.0/3;
    else if (weight < 6)
        return 3.0/8;
    else if (weight <14)
        return 5.0/8;
    else if (weight < 20)
        return 2.0/3;
    else if (weight < 25)
        return 3.0/4;
    else
        return 1.25;
}

double Cat::calculateSleep()
{
    // Calculate the sleep based on age
    if (age < 3)
    {
        energyProgress = 6; maxEnergy = 6;
        return 18;
    }
    else if (age < 10)
    {
        energyProgress = 10; maxEnergy = 10;
        return 14;
    }
    else
    {
        energyProgress = 6; maxEnergy = 6;
        return 18;
    }
}

