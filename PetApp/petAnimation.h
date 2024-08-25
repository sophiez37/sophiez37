#ifndef PETANIMATION_H
#define PETANIMATION_H

#include <QPixmap>
#include <QVector>
#include <QString>

/**
 * @brief The PetAnimation class, store the animation of the of the pet
 */
class PetAnimation
{
public:
    enum PetType {dog, cat}; // Pet type

    /**
     * @brief PetAnimation constructor
     * @param pet the pet type
     */
    PetAnimation (PetType pet);

    /**
     * @brief PetAnimation default constructor
     */
    PetAnimation ();

    // Get the animation sequences
    QVector<QPixmap>& getMovingSeq();  // Return the pet moving sequence
    QVector<QPixmap>& getIdleSeq();    // Return the pet idle state sequence
    QVector<QPixmap>& getEatingSeq();  // Return the pet eating sequence
    QVector<QPixmap>& getLyingSeq();   // Return the pet lying sequence
    QVector<QPixmap>* currentSeq;      // Return the currently displaying sequence of pet

    int nextXPosition();               // Return next X position of the pet
    bool restingMode;                  // Check if the pet is in resting mod

private:
    /**
     * @brief makeAnimation make the anition of the pet
     * @param actionFile the file path of the sequence
     * @param sequenceCount the length of the sequence
     * @param actionSeq the action (moving/idle/etc.)
     * @param margin the margin of the sequence
     */
    void makeAnimation(QString actionFile, int sequenceCount, QVector<QPixmap>& actionSeq, std::tuple<int,int,int,int> margin);

    PetType pet;  // The pet

    QVector<QPixmap> movingSeq;  // The moving sequence
    QVector<QPixmap> idleSeq;    // The idle state sequence
    QVector<QPixmap> eatingSeq;  // The eating state sequence
    QVector<QPixmap> lyingSeq;   // The lying state sequence

};
#endif // PETANIMATION_H
