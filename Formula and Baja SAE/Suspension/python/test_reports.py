from engineering_reports import EngineeringReportEngine


def main():
    e = EngineeringReportEngine()
    r1 = e.report_2d(60.0, 80.0, 450.0)
    assert 0 <= r1.score <= 100
    assert "Reimpell" in r1.to_text()
    r2 = e.report_2d(-100.0, 400.0, 450.0)
    assert r2.score < r1.score
    assert "Piorou" in r2.comparison
    print("REPORT_TESTS_OK")


if __name__ == "__main__":
    main()
