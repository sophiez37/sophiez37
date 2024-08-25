#include "sizedialog.h"
#include "ui_sizedialog.h"
#include "mainmodel.h"
#include "mainwindow.h"

SizeDialog::SizeDialog(QWidget *parent)
    : QDialog(parent)
    , ui(new Ui::SizeDialog)
{
    ui->setupUi(this);

    // Set minimum and maximum size to 2 and 32 respectively
    ui->sizeSlider->setMinimum(2);
    ui->sizeSlider->setMaximum(32);

    // Connect UI inputs to appropriate slots
    connect(ui->sizeSlider, &QSlider::valueChanged, this, &SizeDialog::sliderValueChanged);
    connect(ui->buttonBox, &QDialogButtonBox::accepted, this, &SizeDialog::acceptButtonClicked);
    connect(ui->buttonBox, &QDialogButtonBox::rejected, this, &SizeDialog::rejectButtonClicked);
}

SizeDialog::~SizeDialog()
{
    delete ui;
}

void SizeDialog::sliderValueChanged(int value)
{
    QString text = "Sprite size = " + QString::number(value) + " x " + QString::number(value);
    ui->sizeLabel->setText(text);
    size = value;
}

void SizeDialog::acceptButtonClicked()
{
    MainModel* model = new MainModel(nullptr, size);
    MainWindow* window = new MainWindow(*model);
    window->show();
    this->hide();
}

void SizeDialog::rejectButtonClicked()
{
    QApplication::quit();
}

