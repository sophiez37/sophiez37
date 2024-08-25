#include "mainmodel.h"
#include <QJsonDocument>
#include <QFileDialog>
#include <QJsonObject>
#include <QJsonArray>

MainModel::MainModel(string name, int age, double weight, string animal) : world(b2Vec2(0.0f, 50.0f)),
                                                                           timer(this)
{
    if (animal == "Dog")
    {
        pet = new Dog(name, age, weight);
        pet->animal = "Dog";
    }
    else if(animal == "Cat")
    {
        pet = new Cat(name, age, weight);
        pet->animal = "Cat";
    }

    // Set initial decay wait time
    decayWaitTime = 3000;
    gameTimer.start(decayWaitTime);
    connect(&gameTimer, &QTimer::timeout, this, &MainModel::decayAllBars);
    connect(&gameTimer, &QTimer::timeout, this, &MainModel::updateGameTime);
    selectedAction = Default;

    // Set up box2d
    setBox2dWorld();
    connect(&timer, &QTimer::timeout, this, &MainModel::updateBall);
}

void MainModel::setBox2dWorld()
{
    b2BodyDef ball;
    ball.linearDamping = 0.1;
    ball.position.Set ((50 + 100) / 30.0 , 600 / 30.0);
    body = world.CreateBody(&ball);

    // Define the ground body.
    b2BodyDef groundBodyDef;
    groundBodyDef.position.Set(0.0f, 19.0f);

    b2BodyDef skyBodyDef;
    skyBodyDef.position.Set(0.0f, 0.0f);

    b2BodyDef leftWallBodyDef;
    leftWallBodyDef.position.Set(0.0f, 19.0f);

    b2BodyDef rightWallBodyDef;
    rightWallBodyDef.position.Set(33.5f, 19.0f);

    // Call the body factory which allocates memory for the ground body
    // from a pool and creates the ground box shape (also from a pool).
    // The body is also added to the world.
    b2Body* groundBody = world.CreateBody(&groundBodyDef);
    b2Body* leftWallBody = world.CreateBody(&leftWallBodyDef);
    b2Body* rightWallBody = world.CreateBody(&rightWallBodyDef);
    b2Body* skyWallBody = world.CreateBody(&skyBodyDef);

    // Define the ground box shape.
    b2PolygonShape groundBox;
    b2PolygonShape leftBox;
    b2PolygonShape rightBox;
    b2PolygonShape skyBox;

    // The extents are the half-widths of the box.
    groundBox.SetAsBox(50.0f, 0.1f);
    leftBox.SetAsBox(0.1f, 50.0f);
    rightBox.SetAsBox(0.1f, 50.0f);
    skyBox.SetAsBox(50.0f, 0.1f);

    // Add the ground fixture to the ground body.
    groundBody->CreateFixture(&groundBox, 0.0f);
    leftWallBody->CreateFixture(&leftBox, 0.0f);
    rightWallBody->CreateFixture(&rightBox, 0.0f);
    skyWallBody->CreateFixture(&skyBox, 0.0f);

    // Define the dynamic body. We set its position and call the body factory.
    b2BodyDef bodyDef;
    bodyDef.type = b2_dynamicBody;
    bodyDef.position.Set(0.0f, 12.0f);
    body = world.CreateBody(&bodyDef);

    // Define another box shape for our dynamic body.
    b2CircleShape ballShape;
    ballShape.m_p.Set(0, 0);
    ballShape.m_radius = 1;
    b2FixtureDef ballFixtureDef;
    ballFixtureDef.restitution = 0.2;
    ballFixtureDef.shape = &ballShape;
    ballFixtureDef.density = 5.0f;
    ballFixtureDef.friction = 10;
    body->CreateFixture(&ballFixtureDef);
}

void MainModel::produceNewBall(int x, int y)
{
    if (!timer.isActive())
        timer.start(1000/60);

    body->SetTransform(b2Vec2(x/ppm, y/ppm), 0);

    // Change to whatever force or impulse wanted
    body->ApplyLinearImpulse(b2Vec2(50,-100), body->GetWorldCenter(), true);
    b2Vec2 position = body->GetPosition();

    emit newBallPosMap(position.x * ppm, position.y * ppm);
}

void MainModel::stopTimer()
{
    timer.stop();
}

void MainModel::updateBall()
{
    // Prepare for simulation
    float32 timeStep = 1.0f / 60.0f;
    int32 velocityIterations = 8;
    int32 positionIterations = 8;

    // Instruct the world to perform a single step of simulation
    world.Step(timeStep, velocityIterations, positionIterations);

    // Emit the position and angle of the body
    b2Vec2 position = body->GetPosition();
    emit newBallPos(position.x * ppm, position.y * ppm);

    float angle = body->GetAngle();
    emit newBallAngle(angle);
}

void MainModel::updateGameTime()
{
    gameTime += 10; // every {decaywaittime}, 10 minutes in game pass
    int tempGameTime = gameTime % (24 * 60);
    int day = (gameTime / (24 * 60)) + 1;
    int hours = tempGameTime / 60;
    int minutes = tempGameTime % 60;

    if (hours == 0 && minutes == 0)
    {
        feedingAllowed = true;
        wateringAllowed = true;
    }

    std::string minutesAsString = std::to_string(minutes);
    if (minutesAsString == "0")
        minutesAsString = "00";
    std::string time = std::to_string(hours) + ":" + minutesAsString;
    toJSON();
    emit newGameTime(day, time);
}

void MainModel::setMediumSpeed()
{
    gameTimer.setInterval(1); // Trigger a timeout.
    decayWaitTime = 3000;
}

void MainModel::setFastSpeed()
{
    gameTimer.setInterval(1); // Trigger a time out
    decayWaitTime = 1000;
}

void MainModel::setFasterSpeed()
{
    gameTimer.setInterval(1); // Trigger a timeout
    decayWaitTime = 100;
}

void MainModel::initializeProgressBars()
{
    // Initialize default values to the progress bars
    QVector<int> maxValues;
    for (int x = 0; x < 5; x++)
        maxValues.append(0);

    int maxHunger = pet->calculateFood();
    int maxThirst = pet->calculateWater();
    int maxHygiene = 100;
    int maxPotty = 100;
    int maxEnergy = pet->calculateSleep();

    maxValues[Hunger] = maxHunger;
    maxValues[Thirst] = maxThirst;
    maxValues[Hygiene] = maxHygiene;
    maxValues[Potty] = maxPotty;
    maxValues[Energy] = maxEnergy;

    pet->maxValues = maxValues;

    emit setUpProgressBars(maxValues);
}

void MainModel::decayAllBars()
{
    gameTimer.stop();

    // Decrease percentage
    pet->decreaseHungerProgress(0.00694444444 * decayAllowed); // Every 24 in-game hours, 100% of hunger bar gone.
    pet->decreaseThirstProgress(0.00694444444 * decayAllowed); // Every 24 in-game hours, 100% of thirst bar gone.

    // Decay if pet is not in sleep
    if (petNotSleep)
        pet->decreaseEnergyProgress();
    else
        pet->increaseEnergyProgress();

    pet->decreaseHygieneProgress(50 * decayAllowed);
    pet->decreasePottyProgress();

    // Update the progress bar
    emit updateProgressBar(pet->getHungerPercentage(), Hunger);
    emit updateProgressBar(pet->getThirstPercentage(), Thirst);
    emit updateProgressBar(pet->getEnergyPercentage(), Energy);
    emit updateProgressBar(pet->getHygieneProgress(), Hygiene);
    emit updateProgressBar(pet->getPottyProgress(), Potty);

    toJSON();

    // Start again to repeat the decay
    gameTimer.start(decayWaitTime);
}


void MainModel::getActionSelection(int index)
{
    // Reset the cursor before every action, to avoid cursor overlaying issues
    emit resetCursor();
    if(index == 0)
        selectedAction = Default;
    else if(index == 1)
    {
        selectedAction = Food;
        emit updateFood();
    }
    else if (index == 2)
    {
        selectedAction = Water;
        emit updateWater();
    }
    else if (index == 3)
    {
        selectedAction = Bath;
        emit updateHygiene();
    }
    else if (index == 4)
        selectedAction = Bathroom;
    else if (index == 5)
        selectedAction = Play;
    else if (index == 6)
        selectedAction = Rest;

    toJSON();
    emit setCurrentModule(pet->lessons[selectedAction], selectedAction);
}

// --------- Button event handlers ---------
void MainModel::foodButtonClicked()
{
    getActionSelection(1); // Set to corresponding activity
}

void MainModel::waterButtonClicked()
{
    getActionSelection(2);
}

void MainModel::bathButtonClicked()
{
    getActionSelection(3);
}

void MainModel::pottyButtonClicked()
{
    getActionSelection(4);

}

void MainModel::restButtonClicked(){
    getActionSelection(6);

}

void MainModel::playButtonClicked()
{
    getActionSelection(5);
}

// --------- Decay event handlers ---------
void MainModel::stopAllDecay()
{
    decayAllowed = false;
}

void MainModel::resumeAllDecay()
{
    decayAllowed = true;
}

// --------- Pet sleeping state handlers ---------
void MainModel::petIsSleeping()
{
    petNotSleep = false;
}

void MainModel::petIsNotSleeping()
{
    petNotSleep = true;
}

// Set the file path
void MainModel::setCurrentFilePath(QString filePath)
{
    currentFilePath = filePath;
}

void MainModel::toJSON()
{
    // Initialize Json properties
    QFile clearFile(currentFilePath);
    clearFile.resize(0);
    clearFile.close();
    QJsonArray jsonArray;
    QJsonObject jsonObject;
    QJsonObject jsonPetObject;

    // Insert size of the canvas and current fps to the JSON
    QJsonValue petName = QString::fromStdString(pet->getPetName());
    QJsonValue petType = QString::fromStdString(pet->getPetType());
    jsonPetObject.insert("name", petName);
    jsonPetObject.insert("animal", petType);
    jsonPetObject.insert("weight", pet->getPetWeight());
    jsonPetObject.insert("age", pet->getPetAge());
    jsonPetObject.insert("gametime", gameTime);

    // Add the current frames to a temporary image array
    QJsonArray progressArray;
    progressArray.push_back(pet->getHungerProgress());
    progressArray.push_back(pet->getThirstProgress());
    progressArray.push_back(pet->getPottyProgress());
    progressArray.push_back(pet->getHygieneProgress());
    progressArray.push_back(pet->getEnergyProgress());

    // Insert that image array to JSON
    jsonPetObject.insert("progresses",progressArray);

    // Insert the animation frames
    jsonObject.insert("Pet", jsonPetObject);
    jsonArray.append(jsonObject);

    // Setting up the JSON document
    QJsonDocument jsonDoc;
    jsonDoc.setArray(jsonArray);
    QString strJson(jsonDoc.toJson(QJsonDocument::Indented));

    //Write to file in file path
    QFile currentFile(currentFilePath);
    if (currentFile.open(QIODevice::ReadWrite))
    {
        QTextStream out(&currentFile);
        out << strJson;
    }
    currentFile.close();
}

// Load the progress bar values from file
void MainModel::loadFileProgressbars(QJsonArray progressArray)
{
    pet->setHungerProgress(progressArray.at(Hunger).toDouble());
    pet->setThirstProgress(progressArray.at(Thirst).toDouble());
    pet->setHygieneProgress(progressArray.at(Hygiene).toDouble());
    pet->setPottyProgress(progressArray.at(Potty).toDouble());
    pet->setEnergyProgress(progressArray.at(Energy).toDouble());
}

// --------- Game time handlers ---------
void MainModel::setGameTime(int newGameTime)
{
    gameTime = newGameTime;
    decayAllBars();
    updateGameTime();
}

void MainModel::stopGameTimer()
{
    gameTimer.stop();
}

void MainModel::startGameTimer()
{
    gameTimer.start(decayWaitTime);
}
