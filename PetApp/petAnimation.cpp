#include "PetAnimation.h"
PetAnimation::PetAnimation(PetAnimation::PetType petType) : pet(petType) {}
PetAnimation::PetAnimation()
{
    pet = cat;
}

QVector<QPixmap>& PetAnimation::getMovingSeq()
{
    currentSeq = &movingSeq;
    if (movingSeq.count() != 0)
        return movingSeq;

    if (pet == cat)
        makeAnimation(":/cat_pack/Cat_Walk.png", 8, movingSeq, std::make_tuple(12, 12, 12, 12));
    else // It's a dog
        makeAnimation(":/dog_pack/Dog_Walk.png", 6, movingSeq, std::make_tuple(12, 0, 1, 10));

    return movingSeq;
}

QVector<QPixmap>& PetAnimation::getIdleSeq()
{
    currentSeq = &idleSeq;
    if (idleSeq.count() != 0)
       return idleSeq;

    if (pet == cat)
        makeAnimation(":/cat_pack/Cat_Idle.png", 10, idleSeq, std::make_tuple(12, 12, 12, 12));
    else // It's a dog
        makeAnimation(":/dog_pack/Dog_Idle.png", 4, idleSeq, std::make_tuple(12, 0, 3, 12));

    return idleSeq;
}

QVector<QPixmap>& PetAnimation::getEatingSeq()
{
    currentSeq = &eatingSeq;
    if (eatingSeq.count() != 0)
        return eatingSeq;

    if (pet == cat)
        makeAnimation(":/cat_pack/Cat_Licking.png", 5, eatingSeq, std::make_tuple(12, 12, 12, 12));
    else // It's a dog
        makeAnimation(":/dog_pack/Dog_Attack.png", 4, eatingSeq, std::make_tuple(12, 0, 3, 12));

    return eatingSeq;
}


QVector<QPixmap>& PetAnimation::getLyingSeq()
{
    currentSeq = &lyingSeq;
    if (lyingSeq.count() != 0)
        return lyingSeq;

    if (pet == cat)
        makeAnimation(":/cat_pack/Cat_Laying.png", 8, lyingSeq, std::make_tuple(12, 12, 12, 12));
    else // It's a dog
        makeAnimation(":/dog_pack/Dog_Death.png", 4, lyingSeq, std::make_tuple(12, 0, 3, 0));

    return lyingSeq;
}

void PetAnimation::makeAnimation(QString fileName, int sequenceCount, QVector<QPixmap>& actionSeq, std::tuple<int, int, int, int> margin)
{
    //load cat walking sequence
    QPixmap movementSeq(fileName);
    int frameWidth = movementSeq.width() / sequenceCount;
    int frameHeight = movementSeq.height();

    for (int i = 0; i < sequenceCount; ++i)
    {
        // Calculate the topleft
        int x = i * frameWidth + get<2>(margin);
        int y = get<0>(margin);

        // Adjust frame size
        int adjustedWidth = frameWidth - (get<2>(margin) + get<3>(margin));
        int adjustedHeight = frameHeight - (get<0>(margin) + get<1>(margin));

        QRect frameRect(x, y, adjustedWidth, adjustedHeight);

        // Crop and store
        actionSeq.push_back(movementSeq.copy(frameRect));
    }
}

int PetAnimation::nextXPosition()
{
    return (std::rand() % 51) * 10;
}
