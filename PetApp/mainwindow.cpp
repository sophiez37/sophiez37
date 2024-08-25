#include "dialog.h"
#include "ui_mainwindow.h"
#include "mainwindow.h"
#include <QTimer>
#include <QGraphicsPixmapItem>
#include <QFileDialog>

MainWindow::MainWindow(MainModel& model, string name, int age, double weight, string animal, QWidget *parent)
    : QMainWindow(parent),
    ui(new Ui::MainWindow),
    model(model)
{
    ui->setupUi(this);
    currentAnimal = animal;

    // Scene for background
    scene = new QGraphicsScene(this);

    // petObject for pet sprite, make new for ball.. etc.
    QPixmap catPix(":/cat_pack/Cat_Idle.png");
    petObject = new QGraphicsPixmapItem(catPix);

    QPixmap ballPix(":/icons/ball.png");
    ballObject = new QGraphicsPixmapItem(ballPix.scaled(25, 25));

    // Add sprite to background
    background = new QGraphicsPixmapItem(QPixmap(":icons/livingroom.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));
    scene->addItem(background);

    // Add petObject to scene
    scene->addItem(petObject);
    scene->addItem(ballObject);

    // Display scene to panel
    ui->petPanel->setScene(scene);
    petObject->setPos(0, 0);
    ballObject->setPos(0, 0);
    ballObject->setVisible(false);

    // Display pet info
    QString petInfoText = "Your Pet " + QString::fromStdString(animal) + ": " + QString::fromStdString(name) + "\nAge: " + QString::number(age) + "\nWeight: " + QString::number(weight);
    ui->petNameLabel->setText(petInfoText);

    // Connect events on mouse released/pressed on panel
    connect(ui->petPanel, &PetPanel::mousePressed, this, &MainWindow::panelPressed);
    connect(ui->petPanel, &PetPanel::mouseReleased, this, &MainWindow::panelReleased);

    connect(&model, &MainModel::setUpProgressBars, this, &MainWindow::displayInitialProgressBars);

    // Connect activity buttons to event handler in model
    connect(ui->foodButton, &QPushButton::clicked, &model, &MainModel::foodButtonClicked);
    connect(ui->waterButton, &QPushButton::clicked, &model, &MainModel::waterButtonClicked);
    connect(ui->bathButton, &QPushButton::clicked, &model, &MainModel::bathButtonClicked);
    connect(ui->pottyButton, &QPushButton::clicked, &model, &MainModel::pottyButtonClicked);
    connect(ui->restButton, &QPushButton::clicked, &model, &MainModel::restButtonClicked);
    connect(ui->playButton, &QPushButton::clicked, &model, &MainModel::playButtonClicked);

    // Connect activity buttons to event handler in view
    connect(ui->foodButton, &QPushButton::clicked, this, &MainWindow::foodButtonClicked);
    connect(ui->waterButton, &QPushButton::clicked, this, &MainWindow::waterButtonClicked);
    connect(ui->bathButton, &QPushButton::clicked, this, &MainWindow::bathButtonClicked);
    connect(ui->pottyButton, &QPushButton::clicked, this, &MainWindow::pottyButtonClicked);
    connect(ui->restButton, &QPushButton::clicked, this, &MainWindow::restButtonClicked);
    connect(ui->playButton, &QPushButton::clicked, this, &MainWindow::playButtonClicked);

    // Ball location events
    connect(&model, &MainModel::newBallPos, this, &MainWindow::updateBallPos);
    connect(&model, &MainModel::newBallAngle, this, &MainWindow::updateBallAngle);
    connect(&model, &MainModel::newBallPosMap, this, &MainWindow::updateBallPosMap);
    connect(&model, &MainModel::newBallPos, this, [this](){ ballOnScreen=true;} );
    connect(this, &MainWindow::stopTimer, &model, &MainModel::stopTimer);

    // Ball events when ball out of frame/in frame
    connect(this, &MainWindow::ballOutOfFrame, this, &MainWindow::setBallInvisible);
    connect(this, &MainWindow::ballOutOfFrame, this,[this](){ ballOnScreen=false; } );
    connect(this, &MainWindow::ballOutOfFrame, this,[this](){ touchedBallOnce=false; } );
    connect(this, &MainWindow::ballInFrame, this, &MainWindow::setBallVisible);
    connect(this, &MainWindow::ballInFrame, this,[this](){ ballOnScreen=true; } );
    connect(this, &MainWindow::ballInFrame, this,[this](){ touchedBallOnce=true; } );
    connect(this, &MainWindow::newClickBallPos, &model, &MainModel::produceNewBall);

    // Progress bar events: progress bar complete and update progress bar
    connect(&model, &MainModel::progressBarComplete, this, &MainWindow::showCongratulations);
    connect(&model, &MainModel::updateProgressBar, this, &MainWindow::updateProgressBar);

    connect(&model, &MainModel::setCurrentModule, this, &MainWindow::setCurrentModule);

    // Cursor display for when activity is food/water/hygiene
    connect(&model, &MainModel::updateFood, this, &MainWindow::setFoodDisplay);
    connect(&model, &MainModel::updateWater, this, &MainWindow::setWaterDisplay);
    connect(&model, &MainModel::updateHygiene, this, &MainWindow::setHygieneDisplay);

    // Reset cursor display
    connect(&model, &MainModel::resetCursor, this, &MainWindow::resetCursorDisplay);

    // Pet sleep/not sleep events
    connect(this, &MainWindow::petIsSleeping, &model, &MainModel::petIsSleeping);
    connect(this, &MainWindow::petIsNotSleeping, &model, &MainModel::petIsNotSleeping);

    // Timer events
    connect(this, &MainWindow::stopGameTimer, &model, &MainModel::stopGameTimer);
    connect(&notificationMessageBox, &QMessageBox::buttonClicked, &model, &MainModel::startGameTimer);

    // Set up for time.
    connect(&model, &MainModel::newGameTime, this, &MainWindow::displayGameTime);
    connect(ui->mediumSpeedButton, &QPushButton::clicked, &model, &MainModel::setMediumSpeed);
    connect(ui->fastSpeedButton, &QPushButton::clicked, &model, &MainModel::setFastSpeed);
    connect(ui->fasterSpeedButton, &QPushButton::clicked, &model, &MainModel::setFasterSpeed);
    connect(ui->mediumSpeedButton, &QPushButton::clicked, this, &MainWindow::mediumSpeedClicked);
    connect(ui->fastSpeedButton, &QPushButton::clicked, this, &MainWindow::fastSpeedClicked);
    connect(ui->fasterSpeedButton, &QPushButton::clicked, this, &MainWindow::fasterSpeedClicked);

    // Pet interaction
    connect(this, &MainWindow::interactingWithPet, &model, &MainModel::stopAllDecay);
    connect(this, &MainWindow::stopInteractingWithPet, &model, &MainModel::resumeAllDecay);

    // Save and load pet
    connect(ui->actionNew_Pet, &QAction::triggered, this, &MainWindow::newPet);
    connect(ui->actionLoad_Pet, &QAction::triggered, this, &MainWindow::loadPet);

    model.initializeProgressBars();
    model.getActionSelection(0);

    // Set up buttons.
    if(animal == "Cat") // If we are having cat
    {
        animation = PetAnimation(PetAnimation::cat);

        QPixmap pix(":/icons/cat-food.png");
        QIcon icon(pix);
        ui->foodButton->setIcon(icon);
        ui->foodButton->setIconSize(QSize(100, 100));
        ui->foodButton->setStyleSheet("background-color: rgba(0,0,0,0)");
        actionIndex = 0;
        petObject->setPixmap(animation.getIdleSeq()[actionIndex].scaled(150, 150));
    }
    else // If we are having dog
    {
        animation = PetAnimation(PetAnimation::dog);
        QPixmap pix(":/icons/dog-food.png");
        QIcon icon(pix);
        ui->foodButton->setIcon(icon);
        ui->foodButton->setIconSize(QSize(100, 100));
        ui->foodButton->setStyleSheet("background-color: rgba(0,0,0,0)");
        actionIndex = 0;
        petObject->setPixmap(animation.getIdleSeq()[actionIndex].scaled(120, 120));
    }
    petObject->setPos(100, 260);

    // Water button
    QPixmap waterPix(":/icons/water-cup.png");
    QIcon waterIcon(waterPix);
    ui->waterButton->setIcon(waterIcon);
    ui->waterButton->setIconSize(QSize(100, 100));
    ui->waterButton->setStyleSheet("background-color: rgba(0,0,0,0)");

    // Play button
    QPixmap playPix(":/icons/play.png");
    QIcon ballIcon(playPix);
    ui->playButton->setIcon(ballIcon);
    ui->playButton->setIconSize(QSize(100, 100));
    ui->playButton->setStyleSheet("background-color: rgba(0,0,0,0)");

    // Potty button
    QPixmap pottyPix(":/icons/poop.png");
    QIcon pottyIcon(pottyPix);
    ui->pottyButton->setIcon(pottyIcon);
    ui->pottyButton->setIconSize(QSize(100, 100));
    ui->pottyButton->setStyleSheet("background-color: rgba(0,0,0,0)");

    // Bath button
    QPixmap bathPix(":/icons/bath.png");
    QIcon bathIcon(bathPix);
    ui->bathButton->setIcon(bathIcon);
    ui->bathButton->setIconSize(QSize(100, 100));
    ui->bathButton->setStyleSheet("background-color: rgba(0,0,0,0)");

    // Sleep button
    QPixmap bedPix(":/icons/pet-bed.png");
    QIcon bedIcon(bedPix);
    ui->restButton->setIcon(bedIcon);
    ui->restButton->setIconSize(QSize(100, 100));
    ui->restButton->setStyleSheet("background-color: rgba(0,0,0,0)");

    ui->lessonLabel->setWordWrap(true);
    ui->lessonLabel->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Minimum);

    // Set borders to button, labels and panel
    ui->petNameLabel->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->foodButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->waterButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->bathButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->pottyButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->restButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->playButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->timeLabel->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->fasterSpeedButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->mediumSpeedButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->fastSpeedButton->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");

    ui->lessonLabel->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");
    ui->moduleLabel->setStyleSheet("border: 2px solid #164552; border-radius: 10px;");

    // Connect cat animation
    QTimer *animationTimer= new QTimer();
    connect(animationTimer, &QTimer::timeout,this, [&,animal](){ playPetAnimation(animal); });
    animationTimer->start(100);

    // Connect position timer to move animal around
    QTimer *positionTimer= new QTimer();
    connect(positionTimer, &QTimer::timeout,this, &MainWindow::getNewXPosition);
    positionTimer->start(5000);

    // Set up sound effects.
    backgroundMusicOutput = new QAudioOutput();
    foodAudioOutput = new QAudioOutput();
    congratulationsAudioOutput = new QAudioOutput();
    clickAudioOutput = new QAudioOutput();
    waterAudioOutput = new QAudioOutput();
    fartAudioOutput = new QAudioOutput();
    purrOutput = new QAudioOutput();
    brushOutput = new QAudioOutput();

    backgroundMusicSound = new QMediaPlayer();
    foodSoundEffect = new QMediaPlayer();
    waterSoundEffect = new QMediaPlayer();
    congratulationsSoundEffect = new QMediaPlayer();
    clickSoundEffect = new QMediaPlayer();
    fartSoundEffect = new QMediaPlayer();
    purrEffect = new QMediaPlayer();
    brushEffect = new QMediaPlayer();

    // Set up sound effect output
    congratulationsSoundEffect->setAudioOutput(congratulationsAudioOutput);
    congratulationsSoundEffect->setSource(QUrl(QStringLiteral("qrc:/sounds/congratulations.wav")));

    foodSoundEffect->setAudioOutput(foodAudioOutput);
    foodSoundEffect->setSource(QUrl(QStringLiteral("qrc:/sounds/pouring_food.mp3")));

    waterSoundEffect->setAudioOutput(waterAudioOutput);
    waterSoundEffect->setSource(QUrl(QStringLiteral("qrc:/sounds/pouring_water.mp3")));

    clickSoundEffect->setAudioOutput(clickAudioOutput);
    clickSoundEffect->setSource(QUrl(QStringLiteral("qrc:/sounds/click.mp3")));

    backgroundMusicSound->setAudioOutput(backgroundMusicOutput);
    backgroundMusicSound->setSource(QUrl(QStringLiteral("qrc:/sounds/background_music.mp3")));

    fartSoundEffect->setAudioOutput(fartAudioOutput);
    fartSoundEffect->setSource(QUrl(QStringLiteral("qrc:/icons/fart.mp3")));

    purrEffect->setAudioOutput(purrOutput);
    purrEffect->setSource(QUrl(QStringLiteral("qrc:/sounds/cat-purr.mp3")));

    brushEffect->setAudioOutput(brushOutput);
    brushEffect->setSource(QUrl(QStringLiteral("qrc:/sounds/brushing-cat.mp3")));

    // Setup volume
    backgroundMusicOutput->setVolume(0.2);
    foodAudioOutput->setVolume(0.4);
    waterAudioOutput->setVolume(0.4);
    congratulationsAudioOutput->setVolume(0.3);
    clickAudioOutput->setVolume(0.2);
    fartAudioOutput->setVolume(0.5);
    purrOutput->setVolume(1);
    brushOutput->setVolume(1);

    // Setup backfround music
    connect(backgroundMusicSound, &QMediaPlayer::mediaStatusChanged, this, &MainWindow::replayBackgroundMusic);
    backgroundMusicSound->play();

    ui->timeLabel->setText("Day 1\n" + QString("Time: ") + QString::fromStdString("6:00"));
    ui->mediumSpeedButton->setEnabled(false);

    ballOnScreen = false;
}

void MainWindow::mediumSpeedClicked()
{
    ui->mediumSpeedButton->setEnabled(false);
    ui->fastSpeedButton->setEnabled(true);
    ui->fasterSpeedButton->setEnabled(true);
    clickSoundEffect->play();
}

void MainWindow::fastSpeedClicked()
{
    ui->mediumSpeedButton->setEnabled(true);
    ui->fastSpeedButton->setEnabled(false);
    ui->fasterSpeedButton->setEnabled(true);
    clickSoundEffect->play();
}

void MainWindow::fasterSpeedClicked()
{
    ui->mediumSpeedButton->setEnabled(true);
    ui->fastSpeedButton->setEnabled(true);
    ui->fasterSpeedButton->setEnabled(false);
    clickSoundEffect->play();
}

void MainWindow::displayGameTime(int day, string time)
{
    ui->timeLabel->setText("Day " + QString::number(day) + "\nTime: " + QString::fromStdString(time));

    if (time == "7:00" || time == "19:00")
    {
        emit stopGameTimer();
        notificationMessageBox.setHidden(false);
        notificationMessageBox.setText("Time to feed your pet");
    }

    if (time == "20:00" && currentAnimal == "Cat")
    {
        emit stopGameTimer();
        notificationMessageBox.setHidden(false);
        notificationMessageBox.setText("Time to clean your cat's litter box");
    }

    if (time == "16:00" && currentAnimal == "Dog")
    {
        emit stopGameTimer();
        notificationMessageBox.setHidden(false);
        notificationMessageBox.setText("Time to walk your dog");
    }
}

void MainWindow::replayBackgroundMusic(QMediaPlayer::MediaStatus status)
{
    if(status == QMediaPlayer::EndOfMedia)
    {
        backgroundMusicSound->setPosition(0);
        backgroundMusicSound->play();
    }
}

void MainWindow::playFoodSound()
{
    foodSoundEffect->play();
}

void MainWindow::playClickSound()
{
    clickSoundEffect->play();
}

void MainWindow::displayInitialProgressBars(QVector<int> maxValues)
{
    // Set maximum and minimum of the progress bar
    ui->hungerProgressBar->setMaximum(100);
    ui->hungerProgressBar->setMinimum(0);
    ui->thirstProgressBar->setMaximum(100);
    ui->thirstProgressBar->setMinimum(0);
    ui->hygieneProgressBar->setMaximum(1000);
    ui->pottyProgressBar->setMaximum(100);
    ui->energyProgressBar->setMaximum(100);

    // Set initial value of the progress bars
    ui->hungerProgressBar->setValue(model.pet->getHungerProgress());
    ui->thirstProgressBar->setValue(model.pet->getThirstProgress());
    ui->hygieneProgressBar->setValue(model.pet->getHygieneProgress());
    ui->pottyProgressBar->setValue(model.pet->getPottyProgress());
    ui->energyProgressBar->setValue(model.pet->getEnergyPercentage() * 100);
}

void MainWindow::updateProgressBar(double percentage, MainModel::ProgressBars bar)
{
    if (bar == MainModel::Hunger)
        ui->hungerProgressBar->setValue(percentage * 100);
    else if (bar == MainModel::Thirst)
        ui->thirstProgressBar->setValue(percentage * 100);
    else if (bar == MainModel::Hygiene)
        ui->hygieneProgressBar->setValue(percentage);
    else if (bar == MainModel::Potty)
        ui->pottyProgressBar->setValue(percentage);
    else if (bar == MainModel::Energy)
    {
        ui->energyProgressBar->setValue(percentage * 100);
        if (ui->energyProgressBar->value() <= 0)
            ui->restButton->click();
    }
}

void MainWindow::setCurrentModule(QString lesson, MainModel::Actions action)
{

    ui->lessonLabel->setText(lesson);
    if (action == MainModel::Food)
    {
        emit petIsNotSleeping();
        isMovingToNextRoom = true;
        animation.restingMode = false;
        movingToNextRoom(MainModel::Food);
    }
    else if (action == MainModel::Water)
    {
        emit petIsNotSleeping();
        isMovingToNextRoom = true;
        animation.restingMode = false;
        movingToNextRoom(MainModel::Water);
    }
    else if (action == MainModel::Bath)
    {
        emit petIsNotSleeping();
        isMovingToNextRoom = true;
        animation.restingMode = false;
        movingToNextRoom( MainModel::Bath);
    }
    else if (action == MainModel::Bathroom)
    {
        emit petIsNotSleeping();
        isMovingToNextRoom = true;
        animation.restingMode = false;
        movingToNextRoom(MainModel::Bathroom);
    }
    else if (action == MainModel::Play)
    {
        emit petIsNotSleeping();
        isMovingToNextRoom = true;
        animation.restingMode = false;
        movingToNextRoom(MainModel::Play);
    }
    else if (action == MainModel::Rest)
    {
        emit petIsNotSleeping();
        ballOnScreen = false;
        isMovingToNextRoom = true;
        animation.restingMode = false;
        movingToNextRoom(MainModel::Rest);
        actionIndex = 0;
        emit petIsSleeping();
    }
    else if (action == MainModel::Default)
        emit petIsNotSleeping();
}

void MainWindow::getNewXPosition()
{
    if (!isMovingToNextRoom)
        petXPosition = animation.nextXPosition();
}


void MainWindow:: playPetAnimation(string animal)
{
    int offset = 0;
    int targetPos;
    if (ballOnScreen) // Check if ball is on screen
        targetPos = int(ballObject->x() / 10) * 10 - 90;
    else
        targetPos = petXPosition;

    if (animation.restingMode) // If pet is in resting mode, sleep
        animation.getLyingSeq();

    else if (petObject->x() < targetPos)
    {
        if (isMovingToNextRoom)
            offset = 20;
        else
            offset = 10;

        animationNeedFLip = false; // Turn around
        animation.getMovingSeq(); // Get the animation of the pet
    }

    else if (petObject->x() > targetPos)
    {
        touchedBallOnce = true;
        if (isMovingToNextRoom)
            offset = -20;
        else offset = -10;
        animationNeedFLip = true;
        animation.getMovingSeq();
    }
    else
    {
        offset = 0;
        animation.getIdleSeq(); // Get idle state of pet
    }

    if (actionIndex >= animation.currentSeq->count() - 1 && animation.restingMode)
        actionIndex = animation.currentSeq->count()-1;
    else if (actionIndex >= animation.currentSeq->count() - 1)
        actionIndex = 0;
    else
        actionIndex ++;

    if (animal == "Cat")
    {
        if (animationNeedFLip & touchedBallOnce)
            petObject->setPixmap((*animation.currentSeq)[actionIndex].scaled(150, 150).transformed(QTransform().scale(-1, 1)));
        else
            petObject->setPixmap((*animation.currentSeq)[actionIndex].scaled(150, 150));
        petObject->setPos(petObject->x() + offset, 260);
    }
    else
    {
        if (animationNeedFLip & touchedBallOnce)
            petObject->setPixmap((*animation.currentSeq)[actionIndex].scaled(120, 120).transformed(QTransform().scale(-1, 1)));
        else
            petObject->setPixmap((*animation.currentSeq)[actionIndex].scaled(120, 120));
        petObject->setPos(petObject->x() + offset, 250);
    }

}

void MainWindow::movingToNextRoom(MainModel::Actions action)
{
    // If the action is bath and want to move to potty, update the lesson
    if (currentActivity == MainModel::Bath && action == MainModel::Bathroom)
    {
        fartSoundEffect->play();
        ui->moduleLabel->setText("Lesson: How much poopy a day?");

        model.pet->setPottyProgress(100);
        updateProgressBar(model.pet->getPottyProgress(), MainModel::Potty);
    }

    disableAllButton();

    // If the action is done in same room, do not go to the next room
    if (currentActivity == action ||
        currentActivity == MainModel::Water & action == MainModel::Food ||
        currentActivity == MainModel::Food & action == MainModel::Water ||
        currentActivity == MainModel::Bath & action == MainModel::Bathroom||
        currentActivity == MainModel::Bathroom & action == MainModel::Bath)
    {
        isMovingToNextRoom=false;
        return;
    }

    // Set the petX position
    petXPosition = 600;
    QTimer* checkPosition= new QTimer();

    // Check position is in bound
    connect(checkPosition, &QTimer::timeout, this, [this,checkPosition, action](){ checkPetMovingOut(checkPosition,action); });
    checkPosition->start(100);
    inNewRoom = false;
    currentActivity = action;
}

void MainWindow::checkPetMovingOut(QTimer* timer, MainModel::Actions action)
{
    // Set room to appropriate background
    if(petObject->x() >= petXPosition && !inNewRoom)
    {
        inNewRoom = true;
        switch (action)
        {
        case MainModel::Default:
            break;
        case MainModel::Food:
            background->setPixmap(QPixmap(":icons/livingroom.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));
            break;
        case MainModel::Water:
            background->setPixmap(QPixmap(":icons/livingroom.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));
            break;

        case MainModel:: Bath:
            background->setPixmap(QPixmap(":icons/bathroom.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));
            break;
        case MainModel::Bathroom:
            background->setPixmap(QPixmap(":icons/bathroom.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));

            fartSoundEffect->play();
            ui->moduleLabel->setText("Lesson: How much poopy a day???");

            model.pet->setPottyProgress(100);
            updateProgressBar(model.pet->getPottyProgress(), MainModel::Potty);

            break;
        case MainModel:: Play:
            background->setPixmap(QPixmap(":icons/backyard.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));
            break;
        case MainModel:: Rest:
            background->setPixmap(QPixmap(":icons/bedroom.png").scaled(QSize(680, 390), Qt::IgnoreAspectRatio));
            animation.restingMode=true;
            emit petIsSleeping();
            break;
        }
        timer->stop();
        delete timer;
        petObject->setPos(0, 260);
        petXPosition = 100;
        QTimer* checkPosition= new QTimer();
        connect(checkPosition, &QTimer::timeout, this, [this,checkPosition, action](){ checkPetMovingIn(checkPosition, action); });
        checkPosition->start(100);
    }
}

void MainWindow::disableAllButton()
{
    // Disable all button
    ui->foodButton->setEnabled(false);
    ui->waterButton->setEnabled(false);
    ui->bathButton->setEnabled(false);
    ui->pottyButton->setEnabled(false);
    ui->playButton->setEnabled(false);
    ui->restButton->setEnabled(false);
}

void MainWindow::reEnableAllButton()
{
    // enable all button, but disable the current activity button
    ui->foodButton->setEnabled(true);
    ui->waterButton->setEnabled(true);
    ui->bathButton->setEnabled(true);
    ui->pottyButton->setEnabled(true);
    ui->playButton->setEnabled(true);
    ui->restButton->setEnabled(true);
    switch (currentActivity)
    {
    case MainModel::Default:
        break;
    case MainModel::Food:
        ui->foodButton->setEnabled(false);
        break;
    case MainModel::Water:
        ui->waterButton->setEnabled(false);
        break;
    case MainModel:: Bath:
        ui->bathButton->setEnabled(false);
        break;
    case MainModel::Bathroom:
        ui->pottyButton->setEnabled(false);
        break;
    case MainModel:: Play:
        ui->playButton->setEnabled(false);
        break;
    case MainModel:: Rest:
        ui->restButton->setEnabled(false);
        break;
    }
}

void MainWindow::checkPetMovingIn(QTimer* timer, MainModel::Actions action)
{
    // If pet moved into the room, reenable the buttons
    reEnableAllButton();
    if (petObject->x() >= petXPosition)
    {
        timer->stop();
        delete timer;
        isMovingToNextRoom=false;
    }

    else if (action == MainModel::Rest)
    {
        timer->stop();
        delete timer;
        isMovingToNextRoom=false;
    }
}

void MainWindow::updateBallPos(int newWidth, int newHeight)
{
    ballObject->setPos(newWidth, newHeight);
    ballObject->setVisible(true);
}

void MainWindow::updateBallAngle(double angle)
{
    ballObject->setRotation(angle);
}

void MainWindow::updateBallPosMap(int newWidth, int newHeight)
{
    auto map = ui->petPanel->mapFromScene(newWidth, newHeight);
    updateBallPos(map.x(), map.y());
}

void MainWindow::setBallInvisible()
{
    ui->playButton->setEnabled(true);
    ballObject->setVisible(false);
}

void MainWindow::setBallVisible()
{
    ui->playButton->setEnabled(false);
    ballObject->setVisible(true);
}

void MainWindow::showCongratulations(string message)
{
    // Restore the cursor icon
    QApplication::restoreOverrideCursor();

    // Show message box based on the command
    if (message == "hunger") // Fed the pet to 100
    {
        congratulationsSoundEffect->play();
        congratulationsMessageBox.setHidden(false);
        congratulationsMessageBox.setText("You've given your pet enough food for the day! Do not feed them anymore.");
    }
    else if (message == "thirst") // Water the pet to 100
    {
        congratulationsSoundEffect->play();
        congratulationsMessageBox.setHidden(false);
        congratulationsMessageBox.setText("You've given your pet enough water for the day! Do not water them anymore.");
    }
    else if (message == "no feeding") // No feed after finish the day
    {
        congratulationsMessageBox.setHidden(false);
        congratulationsMessageBox.setText("You've already fed your pet the necessary amount of food. You cannot feed them any more until tomorrow.");
    }
    else if (message == "no watering") // No water after finish the day
    {
        congratulationsMessageBox.setHidden(false);
        congratulationsMessageBox.setText("You've already given your pet the necessary amount of water. You cannot give them any more water until tomorrow.");
    }

    // Stop interacting with pet after showing message box
    emit stopInteractingWithPet();
}

void MainWindow::setFoodDisplay()
{
    // Restore to the original state
    QApplication::restoreOverrideCursor();
    QPixmap p;

    // Set override icon to appropriate icon
    if (currentAnimal == "Cat")
        p = QPixmap(":/icons/fish.png").scaled(50, 50).transformed(QTransform().scale(-1, 1));
    else
        p = QPixmap(":/icons/bone.png").scaled(50, 50).transformed(QTransform().scale(-1, 1));

    QCursor c = QCursor(p, 0, 0);
    QApplication::setOverrideCursor(c);
}

void MainWindow::setHygieneDisplay()
{
    // Restore cursor
    QApplication::restoreOverrideCursor();

    // Set to a new cursor
    QPixmap p = QPixmap(":/icons/brush.png").scaled(50, 50).transformed(QTransform().scale(-1, 1));
    QCursor c = QCursor(p, 0, 0);
    QApplication::setOverrideCursor(c);
}

void MainWindow::setWaterDisplay()
{
    // Restore cursor
    QApplication::restoreOverrideCursor();

    // Set to new cursor
    QPixmap p = QPixmap(":/icons/water-cup.png").scaled(50, 50).transformed(QTransform().scale(-1, 1));
    QCursor c = QCursor(p, 0, 0);
    QApplication::setOverrideCursor(c);
}

void MainWindow::resetCursorDisplay()
{
    // Reset the cursor display to original
    ballObject->setVisible(false);
    ballOnScreen = false;
    emit stopTimer();
    if (model.selectedAction != MainModel::Play)
        ui->playButton->setEnabled(true);

    QApplication::restoreOverrideCursor();
}

void MainWindow::panelPressed(QPoint mouseLocation)
{
    // Allows interaction with pet
    emit interactingWithPet();

    if (model.selectedAction == MainModel::Food)
    {
        if (isMovingToNextRoom)
            return;

        if (model.feedingAllowed) // Feed the pet
        {
            if (petObject->isUnderMouse())
            {
                if (!foodSoundEffect->isPlaying()) // Play the feeding sound
                    foodSoundEffect->play();

                // Update progress bar appropriately
                model.pet->increaseHungerProgress();
                if (model.pet->getHungerPercentage() >= 1)
                {
                    model.feedingAllowed = false;
                    showCongratulations("hunger");
                }
                updateProgressBar(model.pet->getHungerPercentage(), MainModel::Hunger);
            }
        }
        else
            showCongratulations("no feeding");
    }

    else if (model.selectedAction == MainModel::Water) // allow watering the pet
    {
        if (isMovingToNextRoom) // stop interacting when moving to the next room
            return;

        if (model.wateringAllowed){
            if (petObject->isUnderMouse())
            {
                if (!waterSoundEffect->isPlaying())
                    waterSoundEffect->play();

                model.pet->increaseThirstProgress();
                if (model.pet->getThirstPercentage() >= 1)
                {
                    model.wateringAllowed = false;
                    showCongratulations("thirst");
                }
                updateProgressBar(model.pet->getThirstPercentage(), MainModel::Thirst);
            }
        }
        else
            showCongratulations("no watering");
    }

    else if (model.selectedAction == MainModel::Bath) // Allow brushing when in bath
    {
        if (isMovingToNextRoom)
            return;

        if (petObject->isUnderMouse())
        {
            if (currentAnimal == "Cat")
                if (!purrEffect->isPlaying())
                    purrEffect->play();

            if (!brushEffect->isPlaying())
                brushEffect->play();
            model.pet->increaseHygieneProgress();
            updateProgressBar(model.pet->getHygieneProgress(), MainModel::Hygiene);
        }
    }

    else if (model.selectedAction == MainModel::Play)
    {
        emit stopTimer();

        auto map = ui->petPanel->mapToScene(mouseLocation.x(), mouseLocation.y());

        // Set location of ball
        if(map.x() <= 665 && map.x() >= 0 && map.y() > 20 && map.y() < 386)
            ballObject->setPos(map.x(), map.y());
        else
            emit newClickBallPos(100, 100);
    }
}

void MainWindow::panelReleased(QPoint mouseLocation)
{

    emit stopInteractingWithPet();

    foodSoundEffect->stop();
    brushEffect->stop();

    // Displaying the ball and ball dropping effect
    if (model.selectedAction == MainModel::Play)
    {
        if (!isMovingToNextRoom)
        {
            auto map = ui->petPanel->mapToScene(mouseLocation.x(), mouseLocation.y());

            if (map.x() <= 665 && map.x() >= 0 && map.y() > 20 && map.y() < 386)
            {
                setBallVisible();
                ballOnScreen = true;
                emit newClickBallPos(map.x(), map.y());
            }
        }
    }
}

void MainWindow::newPet() // Initialize new pet
{
    resetCursorDisplay();
    Dialog* newDialog = new Dialog();
    newDialog->show();
    delete &model;
    this->close();
    delete this;
}

void MainWindow::loadPet() // Load new pet
{
    resetCursorDisplay();
    Dialog* newDialog = new Dialog();
    delete &model;
    newDialog->loadPet();
    delete this;
}

void MainWindow::foodButtonClicked()
{
    // PLay click sound, change the lesson
    playClickSound();
    ui->moduleLabel->setText("Lesson: How to feed your pet?");
    if (isMovingToNextRoom)
        return;

    // Disable the button
    ui->foodButton->setEnabled(false);
    ui->waterButton->setEnabled(true);
    ui->bathButton->setEnabled(true);
    ui->pottyButton->setEnabled(true);
    ui->restButton->setEnabled(true);
    ui->playButton->setEnabled(true);

}

void MainWindow::waterButtonClicked()
{
    // Play click sound, change the lesson
    playClickSound();
    ui->moduleLabel->setText("Lesson: How to give your pet water?");
    if (isMovingToNextRoom)
        return;

    // Disable the button
    ui->foodButton->setEnabled(true);
    ui->waterButton->setEnabled(false);
    ui->bathButton->setEnabled(true);
    ui->pottyButton->setEnabled(true);
    ui->restButton->setEnabled(true);
    ui->playButton->setEnabled(true);

}

void MainWindow::bathButtonClicked()
{
    // Play click sound, change the lesson
    playClickSound();
    ui->moduleLabel->setText("Lesson: How to clean your pet?");
    if (isMovingToNextRoom)
        return;

    // Disable the button
    ui->foodButton->setEnabled(true);
    ui->waterButton->setEnabled(true);
    ui->bathButton->setEnabled(false);
    ui->pottyButton->setEnabled(true);
    ui->restButton->setEnabled(true);
    ui->playButton->setEnabled(true);

}

void MainWindow::pottyButtonClicked()
{
    // Play click sound, change the lesson
    if (isMovingToNextRoom)
        return;

    // Disable the button
    ui->foodButton->setEnabled(true);
    ui->waterButton->setEnabled(true);
    ui->bathButton->setEnabled(true);
    ui->pottyButton->setEnabled(false);
    ui->restButton->setEnabled(true);
    ui->playButton->setEnabled(true);
}

void MainWindow::restButtonClicked()
{
    // Play click sound, change the lesson
    playClickSound();
    ui->moduleLabel->setText("Lesson: How much sleep is best for your pet?");
    if (isMovingToNextRoom)
        return;
}

void MainWindow::playButtonClicked()
{
    // Play click sound, change the lesson
    playClickSound();
    ui->moduleLabel->setText("Lesson: How to play with your pet?");
    if (isMovingToNextRoom)
        return;
}

MainWindow::~MainWindow()
{
    delete ui;
}
