Салют, это резюме по заданию 3 на стажировке, сервис «Банковские счета»

init branch

DateTime.UtcNow; + HasColumnType("timestamptz").HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))

